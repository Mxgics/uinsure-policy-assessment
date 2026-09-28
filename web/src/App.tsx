import { type FormEvent, useEffect, useMemo, useRef, useState } from 'react'
import { ApiError, policyApi } from './api'
import type { CancellationResult, PaymentMethod, Policy, PolicyTerm, SellPolicyInput } from './types'

const today = new Date().toISOString().slice(0, 10)

const demoPolicies = [
  { reference: 'POL-DEMO-AUTO-HH', title: 'Automatic · Household', detail: 'Card · renewal eligible' },
  { reference: 'POL-DEMO-AUTO-BTL', title: 'Automatic · Buy to Let', detail: 'Direct Debit · three holders' },
  { reference: 'POL-DEMO-MANUAL-HH', title: 'Manual · Household', detail: 'Cheque · unpaid renewal' },
  { reference: 'POL-DEMO-MANUAL-BTL', title: 'Manual · Buy to Let', detail: 'Card · unpaid renewal' },
  { reference: 'POL-DEMO-CANCEL-REFUND', title: 'Cancellation · refund', detail: 'Day 15 pro-rata example' },
  { reference: 'POL-DEMO-CANCEL-CLAIMS', title: 'Cancellation · claims', detail: 'Zero-refund example' },
] as const

function shiftDate(value: string, days: number) {
  const date = new Date(`${value}T00:00:00Z`)
  date.setUTCDate(date.getUTCDate() + days)
  return date.toISOString().slice(0, 10)
}

function latestEligibleBirthDate(startDate: string) {
  const [year, month, day] = startDate.split('-').map(Number)
  const birthYear = year - 16
  const isLeap = (value: number) => value % 4 === 0 && (value % 100 !== 0 || value % 400 === 0)
  const latestDay = month === 2 && day === 28 && !isLeap(year) && isLeap(birthYear) ? 29 : day
  return `${birthYear.toString().padStart(4, '0')}-${month.toString().padStart(2, '0')}-${latestDay.toString().padStart(2, '0')}`
}

function lifecycleAvailability(term: PolicyTerm, terms: PolicyTerm[]) {
  const successors = terms.filter(item => item.predecessorTermId === term.id)
  const renewalStart = shiftDate(term.endDate, -30)
  const renewalReason = successors.length > 0
    ? 'A successor term already exists.'
    : term.cancellation
      ? 'Cancelled terms cannot be renewed.'
      : today < renewalStart
        ? `Renewal opens ${renewalStart}.`
        : today > term.endDate
          ? `Renewal closed ${term.endDate}.`
          : `Eligible now through ${term.endDate}.`
  const activeSuccessor = successors.some(item => item.cancellation === null)
  const cancellationReason = term.cancellation
    ? 'This term is already cancelled.'
    : activeSuccessor
      ? 'A term with an active successor cannot be cancelled.'
      : today > term.endDate
        ? 'Expired terms cannot be cancelled.'
        : 'Eligible to quote and cancel.'
  return {
    canRenew: successors.length === 0 && !term.cancellation && today >= renewalStart && today <= term.endDate,
    canCancel: !term.cancellation && !activeSuccessor && today <= term.endDate,
    renewalStart,
    renewalReason,
    cancellationReason,
  }
}

function formatMoney(value: number) {
  return new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' }).format(value)
}

function errorText(error: unknown) {
  if (!(error instanceof ApiError)) return 'Something unexpected happened. Please try again.'
  const validation = Object.values(error.problem.errors ?? {}).flat()
  return validation.length ? validation.join(' ') : error.message
}

export function App() {
  const [policy, setPolicy] = useState<Policy | null>(null)
  const [reference, setReference] = useState('')
  const [busy, setBusy] = useState(false)
  const busyRef = useRef(false)
  const [stale, setStale] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [quotes, setQuotes] = useState<Record<string, CancellationResult>>({})
  const [confirmTerm, setConfirmTerm] = useState<PolicyTerm | null>(null)
  const keepButtonRef = useRef<HTMLButtonElement>(null)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const cancelTriggerRef = useRef<HTMLButtonElement | null>(null)

  useEffect(() => {
    if (!confirmTerm) return
    if (dialogRef.current && !dialogRef.current.open) dialogRef.current.showModal()
    keepButtonRef.current?.focus()
  }, [confirmTerm])

  function closeDialog() {
    setConfirmTerm(null)
    queueMicrotask(() => cancelTriggerRef.current?.focus())
  }

  const sortedTerms = useMemo(
    () => [...(policy?.terms ?? [])].sort((a, b) => a.startDate.localeCompare(b.startDate)),
    [policy],
  )

  async function refreshPolicy(policyReference: string, prefix = '') {
    setQuotes({})
    try {
      setPolicy(await policyApi.get(policyReference))
      setStale(false)
      setError(prefix ? `${prefix} The latest state is now shown.` : '')
    } catch {
      setStale(true)
      setError(`${prefix} Refresh failed. Refresh policy before making further changes.`.trim())
    }
  }

  async function handleActionError(caught: unknown, mutation: false | 'sale' | 'lifecycle') {
    if (caught instanceof ApiError && caught.status === 409 && policy) {
      await refreshPolicy(policy.reference, errorText(caught))
      return
    }

    const outcomeIsUncertain = !(caught instanceof ApiError) || caught.status >= 500
    if (!outcomeIsUncertain) {
      setError(errorText(caught))
      return
    }

    switch (mutation) {
      case 'sale':
        setError('Policy creation may have succeeded, but no usable confirmation was received. Keep these details and do not resubmit: without a known reference, a second request could create a duplicate policy.')
        return
      case 'lifecycle':
        setStale(true)
        setQuotes({})
        setError('The outcome is uncertain. Refresh policy before making further changes; do not repeat the request.')
        return
      default:
        setError(errorText(caught))
    }
  }

  async function run(action: () => Promise<void>, mutation: false | 'sale' | 'lifecycle' = false, preserveNotice = false) {
    if (busyRef.current) return
    busyRef.current = true
    setBusy(true)
    setError('')
    if (!preserveNotice) setNotice('')
    try {
      await action()
    } catch (caught) {
      await handleActionError(caught, mutation)
    } finally {
      setBusy(false)
      busyRef.current = false
    }
  }

  async function loadPolicy(policyReference: string) {
    await run(async () => {
      const found = await policyApi.get(policyReference)
      setPolicy(found)
      setReference(found.reference)
      setQuotes({})
      setStale(false)
      setNotice(`Loaded ${found.reference}.`)
    })
  }

  async function findPolicy(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await loadPolicy(reference)
  }

  async function sellPolicy(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const holderCount = Number(form.get('policyholderCount'))
    const input: SellPolicyInput = {
      type: form.get('type') as Policy['type'],
      startDate: String(form.get('startDate')),
      premium: Number(form.get('premium')),
      hasClaims: form.get('hasClaims') === 'on',
      autoRenew: form.get('renewalMode') === 'automatic',
      policyholders: Array.from({ length: holderCount }, (_, index) => ({
        firstName: String(form.get(`firstName-${index}`)),
        lastName: String(form.get(`lastName-${index}`)),
        dateOfBirth: String(form.get(`dateOfBirth-${index}`)),
      })),
      property: {
        addressLine1: String(form.get('addressLine1')),
        addressLine2: String(form.get('addressLine2')).trim() || null,
        addressLine3: String(form.get('addressLine3')).trim() || null,
        city: String(form.get('city')).trim() || null,
        postcode: String(form.get('postcode')),
      },
      paymentMethod: form.get('paymentMethod') as PaymentMethod,
    }
    await run(async () => {
      const created = await policyApi.sell(input)
      setPolicy(created)
      setReference(created.reference)
      setStale(false)
      setQuotes({})
      setNotice(`Policy ${created.reference} was created.`)
    }, 'sale')
  }

  async function quote(term: PolicyTerm, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const date = String(new FormData(event.currentTarget).get('quoteDate'))
    await run(async () => {
      const result = await policyApi.quote(policy!.reference, term.id, date)
      setQuotes(current => ({ ...current, [term.id]: result }))
      setNotice('This is a hypothetical quote only. No policy data was changed.')
    })
  }

  async function cancelConfirmed() {
    if (!policy || !confirmTerm || stale || busyRef.current) return
    const term = confirmTerm
    const policyReference = policy.reference
    setConfirmTerm(null)
    await run(async () => {
      const result = await policyApi.cancel(policyReference, term.id)
      // Keep the confirmed mutation visible even when the following GET cannot refresh it.
      setPolicy(current => current && ({ ...current, terms: current.terms.map(item => item.id === term.id
        ? { ...item, state: 'Cancelled', cancellation: result } : item) }))
      setQuotes({})
      setNotice('Cancellation recorded using a fresh calculation for today.')
      await refreshPolicy(policyReference)
    }, 'lifecycle')
    requestAnimationFrame(() => document.getElementById(`term-${term.id}`)?.focus())
  }

  async function renew(term: PolicyTerm, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!policy || stale) return
    const policyReference = policy.reference
    const value = String(new FormData(event.currentTarget).get('renewalPayment') ?? '')
    await run(async () => {
      const successor = await policyApi.renew(policyReference, term.id, value || null)
      // Keep the confirmed mutation visible even when the following GET cannot refresh it.
      setPolicy(current => current && ({ ...current,
        terms: [...current.terms.filter(item => item.id !== successor.id), successor] }))
      setQuotes({})
      setNotice('Renewal recorded and the new term is shown in the history.')
      await refreshPolicy(policyReference)
    }, 'lifecycle')
  }

  return (
    <>
      <header className="hero">
        <div className="shell hero__content">
          <div>
            <p className="eyebrow">Technical assessment · local demonstration</p>
            <h1>Policy desk</h1>
            <p className="lede">Sell, find and manage a policy without hiding the lifecycle history.</p>
          </div>
          <div className="demo-badge" aria-label="Demonstration environment">Synthetic data only</div>
        </div>
      </header>

      <main className="shell">
        <section className="search-panel" aria-labelledby="find-heading">
          <div>
            <p className="section-kicker">Existing policy</p>
            <h2 id="find-heading">Find by reference</h2>
          </div>
          <form className="find-form" onSubmit={findPolicy}>
            <label htmlFor="reference">Policy reference</label>
            <div className="inline-control">
              <input id="reference" disabled={busy} value={reference} onChange={event => setReference(event.target.value)} required placeholder="POL-…" />
              <button disabled={busy}>Find policy</button>
            </div>
          </form>
        </section>

        <details className="demo-panel">
          <summary>
            <span><span className="section-kicker">Seeded scenarios</span><strong>Try a prepared policy</strong></span>
            <span>6 scenarios</span>
          </summary>
          <p>Start locally with <code>Start-Local.ps1 -SeedDemo</code>, then load any scenario.</p>
          <div className="demo-grid">
            {demoPolicies.map(item => (
              <button key={item.reference} type="button" className="demo-policy" disabled={busy}
                onClick={event => {
                  const disclosure = event.currentTarget.closest('details')
                  if (disclosure) disclosure.open = false
                  setReference(item.reference)
                  void loadPolicy(item.reference)
                }}>
                <strong>{item.title}</strong>
                <span>{item.detail}</span>
                <small>{item.reference}</small>
              </button>
            ))}
          </div>
        </details>

        <div className="status-stack" aria-live="polite" aria-atomic="true">
          {error && <div className="alert alert--error" role="alert">{error}</div>}
          {notice && <div className="alert alert--success">{notice}</div>}
          {stale && policy && <button disabled={busy} onClick={() => run(() => refreshPolicy(policy.reference), false, true)}>Refresh policy</button>}
        </div>

        {!policy ? <SellForm onSubmit={sellPolicy} busy={busy} /> : (
          <section aria-labelledby="policy-heading">
            <div className="policy-heading">
              <div>
                <p className="section-kicker">{policy.type === 'BuyToLet' ? 'Buy to Let' : 'Household'}</p>
                <h2 id="policy-heading">{policy.reference}</h2>
              </div>
              <button disabled={busy} className="button--quiet" onClick={() => { setPolicy(null); setQuotes({}); setNotice(''); setError(''); setStale(false) }}>Sell another policy</button>
            </div>
            <ol className="timeline" aria-label="Policy term history">
              {sortedTerms.map((term, index) => {
                const availability = lifecycleAvailability(term, sortedTerms)
                return (
                <li key={term.id} className="term-card">
                  <div className="term-card__header">
                    <div>
                      <p className="term-index">Term {index + 1}</p>
                      <h3 id={`term-${term.id}`} tabIndex={-1}>{term.startDate} <span aria-hidden="true">→</span> {term.endDate}</h3>
                    </div>
                    <span className={`state state--${term.state.toLowerCase()}`}>{term.state}</span>
                  </div>
                  <dl className="facts">
                    <div><dt>Premium</dt><dd>{formatMoney(term.premium)}</dd></div>
                    <div><dt>Payment</dt><dd>{term.paymentState}{term.payment ? ` · ${term.payment.method === 'DirectDebit' ? 'Direct Debit' : term.payment.method}` : ''}</dd></div>
                    <div><dt>Claims</dt><dd>{term.hasClaims ? 'Recorded' : 'None'}</dd></div>
                    <div><dt>Renewal mode</dt><dd><span className={`renewal-mode renewal-mode--${term.autoRenew ? 'automatic' : 'manual'}`}>{term.autoRenew ? 'Automatic' : 'Manual'}</span></dd></div>
                    <div><dt>Property</dt><dd>{[term.property.addressLine1, term.property.addressLine2, term.property.addressLine3, term.property.city, term.property.postcode].filter(Boolean).join(', ')}</dd></div>
                  </dl>
                  <div className="holder-summary">
                    <h4>Policy holders ({term.policyholders.length})</h4>
                    <ul>
                      {term.policyholders.map((holder, holderIndex) => (
                        <li key={`${holder.firstName}-${holder.lastName}-${holderIndex}`}>
                          <strong>{holder.firstName} {holder.lastName}</strong><span>Born {holder.dateOfBirth}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  {term.cancellation && (
                    <div className="history-note">Cancelled {term.cancellation.date}; refund {formatMoney(term.cancellation.refundAmount)} ({term.cancellation.reason}).</div>
                  )}
                  {term.state !== 'Cancelled' && (
                    <div className="actions-grid">
                      <form onSubmit={event => quote(term, event)}>
                        <h4>Cancellation estimate</h4>
                        <p>Hypothetical only. Confirmation always recalculates for today.</p>
                        <p className="availability">{availability.cancellationReason}</p>
                        <label htmlFor={`quote-${term.id}`}>Quote date</label>
                        <input id={`quote-${term.id}`} name="quoteDate" type="date" defaultValue={today} required />
                        <button className="button--secondary" disabled={busy || stale || !availability.canCancel}>Get quote</button>
                        {quotes[term.id] && <Quote result={quotes[term.id]} />}
                        <button type="button" className="button--danger" onClick={event => {
                          cancelTriggerRef.current = event.currentTarget
                          setConfirmTerm(term)
                        }} disabled={busy || stale || !availability.canCancel}>Cancel policy term</button>
                      </form>
                      <form onSubmit={event => renew(term, event)}>
                        <h4>{term.autoRenew ? 'Automatic renewal' : 'Manual renewal'}</h4>
                        <p>Window: {availability.renewalStart} through {term.endDate}.</p>
                        <p className="availability">{availability.renewalReason}</p>
                        {term.autoRenew ? (
                          <>
                            <label htmlFor={`renew-${term.id}`}>Payment method</label>
                            <select id={`renew-${term.id}`} name="renewalPayment" defaultValue="Card">
                              <option value="Card">Card</option>
                              <option value="DirectDebit">Direct debit</option>
                            </select>
                          </>
                        ) : <><input type="hidden" name="renewalPayment" value="" /><p>No payment will be recorded for the successor term.</p></>}
                        <button disabled={busy || stale || !availability.canRenew}>{term.autoRenew ? 'Record automatic renewal' : 'Create unpaid renewal'}</button>
                      </form>
                    </div>
                  )}
                </li>
                )
              })}
            </ol>
          </section>
        )}
      </main>

      {confirmTerm && (
          <dialog ref={dialogRef} aria-labelledby="confirm-title" className="dialog" onCancel={event => { event.preventDefault(); closeDialog() }} onClose={closeDialog}
            onKeyDown={event => {
              if (event.key !== 'Tab') return
              const buttons = event.currentTarget.querySelectorAll<HTMLButtonElement>('button:not(:disabled)')
              const first = buttons[0], last = buttons[buttons.length - 1]
              if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus() }
              else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus() }
            }}>
            <p className="section-kicker">Irreversible demo action</p>
            <h2 id="confirm-title">Cancel this policy term?</h2>
            <p>The API will calculate today’s result again. A previous quote is not a guarantee.</p>
            <div className="dialog__actions">
              <button ref={keepButtonRef} className="button--quiet" onClick={closeDialog}>Keep term</button>
              <button disabled={busy} className="button--danger" onClick={cancelConfirmed}>Confirm cancellation</button>
            </div>
          </dialog>
      )}
    </>
  )
}

function Quote({ result }: { result: CancellationResult }) {
  return (
    <div className="quote" aria-label="Cancellation quote result">
      <strong>{formatMoney(result.refundAmount)} refund</strong>
      <span>Calculated for {result.date}</span>
      <span>{result.reason} · {result.unusedDays} unused days</span>
      <small>Retained premium {formatMoney(result.retainedPremium)}; this is not an extra fee.</small>
    </div>
  )
}

function SellForm({ onSubmit, busy }: { onSubmit: (event: FormEvent<HTMLFormElement>) => void; busy: boolean }) {
  const [holderIds, setHolderIds] = useState([0])
  const [nextHolderId, setNextHolderId] = useState(1)
  const [startDate, setStartDate] = useState(today)
  const [renewalMode, setRenewalMode] = useState<'automatic' | 'manual'>('automatic')
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('Card')
  const latestBirthDate = latestEligibleBirthDate(startDate)

  function addHolder() {
    if (holderIds.length >= 3) return
    setHolderIds(current => [...current, nextHolderId])
    setNextHolderId(current => current + 1)
  }

  function removeHolder(id: number) {
    if (holderIds.length <= 1) return
    setHolderIds(current => current.filter(holderId => holderId !== id))
  }

  function selectRenewalMode(mode: 'automatic' | 'manual') {
    setRenewalMode(mode)
    if (mode === 'automatic' && paymentMethod === 'Cheque') setPaymentMethod('Card')
  }

  return (
    <section className="sell-panel" aria-labelledby="sell-heading">
      <div className="sell-intro">
        <p className="section-kicker">New policy</p>
        <h2 id="sell-heading">Create a policy</h2>
        <p>Create a policy with one to three holders, then retrieve it to review the complete lifecycle history.</p>
      </div>
      <form className="sell-form" onSubmit={onSubmit}>
        <input type="hidden" name="policyholderCount" value={holderIds.length} />
        <fieldset>
          <legend>Cover</legend>
          <label>Policy type<select name="type" defaultValue="Household"><option value="Household">Household</option><option value="BuyToLet">Buy to Let</option></select></label>
          <label>Start date<input name="startDate" type="date" value={startDate} min={today} max={shiftDate(today, 60)} onChange={event => setStartDate(event.target.value)} required /></label>
          <label>Annual premium (£)<input name="premium" type="number" min="0.01" max="90071992547409.90" step="0.01" defaultValue="365.00" required /></label>
          <label>Payment method<select name="paymentMethod" value={paymentMethod} onChange={event => setPaymentMethod(event.target.value as PaymentMethod)}><option>Card</option><option value="DirectDebit">Direct debit</option><option disabled={renewalMode === 'automatic'}>Cheque</option></select></label>
          <label className="check"><input name="hasClaims" type="checkbox" /> This term has claims</label>
          <fieldset className="renewal-choice">
            <legend>Renewal mode</legend>
            <label className="check"><input name="renewalMode" type="radio" value="automatic" checked={renewalMode === 'automatic'} onChange={() => selectRenewalMode('automatic')} /> Automatic</label>
            <label className="check"><input name="renewalMode" type="radio" value="manual" checked={renewalMode === 'manual'} onChange={() => selectRenewalMode('manual')} /> Manual</label>
            <small>{renewalMode === 'automatic'
              ? 'An explicit renewal records a Card or Direct Debit payment. Cheque is unavailable.'
              : 'An explicit renewal creates an unpaid successor. Cheque is allowed for this initial payment.'}</small>
          </fieldset>
        </fieldset>
        {holderIds.map((holderId, index) => (
          <fieldset key={holderId} className="policyholder-fieldset">
            <legend>Policy holder {index + 1}</legend>
            <label>First name<input name={`firstName-${index}`} maxLength={100} autoComplete="given-name" required /></label>
            <label>Last name<input name={`lastName-${index}`} maxLength={100} autoComplete="family-name" required /></label>
            <label>Date of birth<input name={`dateOfBirth-${index}`} type="date" max={latestBirthDate} aria-describedby={`dob-help-${holderId}`} required /></label>
            <small id={`dob-help-${holderId}`}>Must be at least 16 on {startDate}.</small>
            {holderIds.length > 1 && <button type="button" className="button--quiet" onClick={() => removeHolder(holderId)}>Remove policy holder {index + 1}</button>}
          </fieldset>
        ))}
        <div className="policyholder-actions">
          <button type="button" className="button--secondary" disabled={holderIds.length >= 3} onClick={addHolder}>Add another policy holder</button>
          <span>{holderIds.length} of 3 policy holders</span>
        </div>
        <fieldset>
          <legend>Property</legend>
          <label>Address line 1<input name="addressLine1" maxLength={200} autoComplete="address-line1" required /></label>
          <label>Address line 2<input name="addressLine2" maxLength={200} autoComplete="address-line2" /></label>
          <label>Address line 3<input name="addressLine3" maxLength={200} autoComplete="address-line3" /></label>
          <label>Town or city<input name="city" maxLength={100} autoComplete="address-level2" /></label>
          <label>Postcode<input name="postcode" autoComplete="postal-code" maxLength={8} required /></label>
        </fieldset>
        <button className="sell-submit" disabled={busy}>{busy ? 'Creating…' : 'Create policy'}</button>
      </form>
    </section>
  )
}
