import { type FormEvent, useEffect, useMemo, useRef, useState } from 'react'
import { ApiError, policyApi } from './api'
import type { CancellationResult, PaymentMethod, Policy, PolicyTerm, SellPolicyInput } from './types'

const today = new Date().toISOString().slice(0, 10)

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
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [quotes, setQuotes] = useState<Record<string, CancellationResult>>({})
  const [confirmTerm, setConfirmTerm] = useState<PolicyTerm | null>(null)
  const confirmButtonRef = useRef<HTMLButtonElement>(null)
  const cancelTriggerRef = useRef<HTMLButtonElement | null>(null)

  useEffect(() => {
    if (!confirmTerm) return
    confirmButtonRef.current?.focus()
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') closeDialog()
    }
    document.addEventListener('keydown', onKeyDown)
    return () => document.removeEventListener('keydown', onKeyDown)
  }, [confirmTerm])

  function closeDialog() {
    setConfirmTerm(null)
    queueMicrotask(() => cancelTriggerRef.current?.focus())
  }

  const sortedTerms = useMemo(
    () => [...(policy?.terms ?? [])].sort((a, b) => a.startDate.localeCompare(b.startDate)),
    [policy],
  )

  async function run(action: () => Promise<void>, conflictMessage?: string) {
    if (busy) return
    setBusy(true)
    setError('')
    setNotice('')
    try {
      await action()
    } catch (caught) {
      if (caught instanceof ApiError && caught.status === 409 && policy) {
        const refreshed = await policyApi.get(policy.reference).catch(() => null)
        if (refreshed) setPolicy(refreshed)
        setError(conflictMessage ?? 'The policy changed. The latest state is now shown.')
      } else {
        setError(errorText(caught))
      }
    } finally {
      setBusy(false)
    }
  }

  async function findPolicy(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    await run(async () => {
      const found = await policyApi.get(reference)
      setPolicy(found)
      setReference(found.reference)
      setNotice(`Loaded ${found.reference}.`)
    })
  }

  async function sellPolicy(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = new FormData(event.currentTarget)
    const input: SellPolicyInput = {
      type: form.get('type') as Policy['type'],
      startDate: String(form.get('startDate')),
      premium: Number(form.get('premium')),
      hasClaims: form.get('hasClaims') === 'on',
      autoRenew: form.get('autoRenew') === 'on',
      policyholders: [{
        firstName: String(form.get('firstName')),
        lastName: String(form.get('lastName')),
        dateOfBirth: String(form.get('dateOfBirth')),
      }],
      property: {
        addressLine1: String(form.get('addressLine1')),
        addressLine2: null,
        city: String(form.get('city')),
        postcode: String(form.get('postcode')),
        bedrooms: Number(form.get('bedrooms')),
      },
      paymentMethod: form.get('paymentMethod') as PaymentMethod,
    }
    await run(async () => {
      const created = await policyApi.sell(input)
      setPolicy(created)
      setReference(created.reference)
      setNotice(`Policy ${created.reference} was created.`)
    })
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
    if (!policy || !confirmTerm) return
    const term = confirmTerm
    setConfirmTerm(null)
    await run(async () => {
      await policyApi.cancel(policy.reference, term.id)
      setPolicy(await policyApi.get(policy.reference))
      setNotice('Cancellation recorded using a fresh calculation for today.')
    }, 'Cancellation was not applied because the policy changed. The latest state is shown.')
  }

  async function renew(term: PolicyTerm, event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const value = String(new FormData(event.currentTarget).get('renewalPayment') ?? '')
    await run(async () => {
      await policyApi.renew(policy!.reference, term.id, value || null)
      setPolicy(await policyApi.get(policy!.reference))
      setNotice('Renewal recorded and the new term is shown in the history.')
    }, 'Renewal was not applied because the policy changed. The latest state is shown.')
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
              <input id="reference" value={reference} onChange={event => setReference(event.target.value)} required placeholder="POL-…" />
              <button disabled={busy}>Find policy</button>
            </div>
          </form>
        </section>

        <div className="status-stack" aria-live="polite" aria-atomic="true">
          {error && <div className="alert alert--error" role="alert">{error}</div>}
          {notice && <div className="alert alert--success">{notice}</div>}
        </div>

        {!policy ? <SellForm onSubmit={sellPolicy} busy={busy} /> : (
          <section aria-labelledby="policy-heading">
            <div className="policy-heading">
              <div>
                <p className="section-kicker">{policy.type === 'BuyToLet' ? 'Buy to Let' : 'Household'}</p>
                <h2 id="policy-heading">{policy.reference}</h2>
              </div>
              <button className="button--quiet" onClick={() => { setPolicy(null); setQuotes({}); setNotice('') }}>Sell another policy</button>
            </div>
            <ol className="timeline" aria-label="Policy term history">
              {sortedTerms.map((term, index) => (
                <li key={term.id} className="term-card">
                  <div className="term-card__header">
                    <div>
                      <p className="term-index">Term {index + 1}</p>
                      <h3>{term.startDate} <span aria-hidden="true">→</span> {term.endDate}</h3>
                    </div>
                    <span className={`state state--${term.state.toLowerCase()}`}>{term.state}</span>
                  </div>
                  <dl className="facts">
                    <div><dt>Premium</dt><dd>{formatMoney(term.premium)}</dd></div>
                    <div><dt>Payment</dt><dd>{term.paymentState}</dd></div>
                    <div><dt>Claims</dt><dd>{term.hasClaims ? 'Recorded' : 'None'}</dd></div>
                    <div><dt>Property</dt><dd>{term.property.postcode} · {term.property.bedrooms} bed</dd></div>
                  </dl>
                  {term.cancellation && (
                    <div className="history-note">Cancelled {term.cancellation.date}; refund {formatMoney(term.cancellation.refundAmount)} ({term.cancellation.reason}).</div>
                  )}
                  {term.state !== 'Cancelled' && (
                    <div className="actions-grid">
                      <form onSubmit={event => quote(term, event)}>
                        <h4>Cancellation estimate</h4>
                        <p>Hypothetical only. Confirmation always recalculates for today.</p>
                        <label htmlFor={`quote-${term.id}`}>Quote date</label>
                        <input id={`quote-${term.id}`} name="quoteDate" type="date" defaultValue={today} required />
                        <button className="button--secondary" disabled={busy}>Get quote</button>
                        {quotes[term.id] && <Quote result={quotes[term.id]} />}
                        <button type="button" className="button--danger" disabled={busy} onClick={event => {
                          cancelTriggerRef.current = event.currentTarget
                          setConfirmTerm(term)
                        }}>Cancel policy term</button>
                      </form>
                      <form onSubmit={event => renew(term, event)}>
                        <h4>Renew this term</h4>
                        <p>Available only during the final 31 calendar dates.</p>
                        {term.autoRenew ? (
                          <>
                            <label htmlFor={`renew-${term.id}`}>Payment method</label>
                            <select id={`renew-${term.id}`} name="renewalPayment" defaultValue="Card">
                              <option value="Card">Card</option>
                              <option value="DirectDebit">Direct debit</option>
                            </select>
                          </>
                        ) : <input type="hidden" name="renewalPayment" value="" />}
                        <button disabled={busy}>Renew term</button>
                      </form>
                    </div>
                  )}
                </li>
              ))}
            </ol>
          </section>
        )}
      </main>

      {confirmTerm && (
        <div className="dialog-backdrop">
          <section role="dialog" aria-modal="true" aria-labelledby="confirm-title" className="dialog">
            <p className="section-kicker">Irreversible demo action</p>
            <h2 id="confirm-title">Cancel this policy term?</h2>
            <p>The API will calculate today’s result again. A previous quote is not a guarantee.</p>
            <div className="dialog__actions">
              <button className="button--quiet" onClick={closeDialog}>Keep term</button>
              <button ref={confirmButtonRef} className="button--danger" onClick={cancelConfirmed}>Confirm cancellation</button>
            </div>
          </section>
        </div>
      )}
    </>
  )
}

function Quote({ result }: { result: CancellationResult }) {
  return (
    <div className="quote" aria-label="Cancellation quote result">
      <strong>{formatMoney(result.refundAmount)} refund</strong>
      <span>{result.reason} · {result.unusedDays} unused days</span>
      <small>Retained premium {formatMoney(result.retainedPremium)}; this is not an extra fee.</small>
    </div>
  )
}

function SellForm({ onSubmit, busy }: { onSubmit: (event: FormEvent<HTMLFormElement>) => void; busy: boolean }) {
  return (
    <section className="sell-panel" aria-labelledby="sell-heading">
      <div className="sell-intro">
        <p className="section-kicker">New policy</p>
        <h2 id="sell-heading">Create a policy</h2>
        <p>This focused demo captures one policyholder. The API supports up to three.</p>
      </div>
      <form className="sell-form" onSubmit={onSubmit}>
        <fieldset>
          <legend>Cover</legend>
          <label>Policy type<select name="type" defaultValue="Household"><option value="Household">Household</option><option value="BuyToLet">Buy to Let</option></select></label>
          <label>Start date<input name="startDate" type="date" defaultValue={today} required /></label>
          <label>Annual premium (£)<input name="premium" type="number" min="0.01" step="0.01" defaultValue="365.00" required /></label>
          <label>Payment method<select name="paymentMethod" defaultValue="Card"><option>Card</option><option value="DirectDebit">Direct debit</option><option>Cheque</option></select></label>
          <label className="check"><input name="hasClaims" type="checkbox" /> This term has claims</label>
          <label className="check"><input name="autoRenew" type="checkbox" defaultChecked /> Auto-renew</label>
        </fieldset>
        <fieldset>
          <legend>Policyholder</legend>
          <label>First name<input name="firstName" autoComplete="given-name" required /></label>
          <label>Last name<input name="lastName" autoComplete="family-name" required /></label>
          <label>Date of birth<input name="dateOfBirth" type="date" required /></label>
        </fieldset>
        <fieldset>
          <legend>Property</legend>
          <label>Address line 1<input name="addressLine1" autoComplete="address-line1" required /></label>
          <label>Town or city<input name="city" autoComplete="address-level2" required /></label>
          <label>Postcode<input name="postcode" autoComplete="postal-code" maxLength={8} required /></label>
          <label>Bedrooms<input name="bedrooms" type="number" min="1" defaultValue="3" required /></label>
        </fieldset>
        <button className="sell-submit" disabled={busy}>{busy ? 'Creating…' : 'Create policy'}</button>
      </form>
    </section>
  )
}
