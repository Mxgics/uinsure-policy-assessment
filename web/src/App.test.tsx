import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { App } from './App'
import { policy, term, quote } from './test-fixtures'

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

describe('policy desk', () => {
  it('provides labelled sell controls and a synthetic-data warning', () => {
    render(<App />)

    expect(screen.getByLabelText('Demonstration environment')).toHaveTextContent('Synthetic data only')
    expect(screen.getByLabelText('Policy type')).toBeVisible()
    expect(screen.getByLabelText('First name')).toBeVisible()
    expect(screen.getByLabelText('Postcode')).toHaveAttribute('maxlength', '8')
    expect(screen.getByLabelText('Annual premium (£)')).toHaveAttribute('max', '90071992547409.90')
  })

  it('presents the date returned with a cancellation quote', async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
      .mockResolvedValueOnce(new Response(JSON.stringify(quote)))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.submit(screen.getByLabelText('Policy reference').closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })

    fireEvent.submit(screen.getByRole('heading', { name: 'Cancellation estimate' }).closest('form')!)

    expect(await screen.findByText(`Calculated for ${quote.date}`)).toBeVisible()
  })

  it('shows an informative API error in a live alert', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({
      title: 'Policy not found',
      detail: 'Check the reference and try again.',
    }), { status: 404, headers: { 'content-type': 'application/json' } })))
    render(<App />)

    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: 'POL-MISSING' } })
    fireEvent.click(screen.getByRole('button', { name: 'Find policy' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Check the reference and try again.')
  })

  it.each([
    ['a network failure', () => Promise.reject(new TypeError('Network failure'))],
    ['an unusable success response', () => Promise.resolve(new Response('{}', { status: 201 }))],
    ['an HTTP 500 response', () => Promise.resolve(new Response('{}', { status: 500 }))],
  ])('treats sale %s as uncertain without inviting a duplicate', async (_description, response) => {
    const fetchMock = vi.fn().mockImplementation(response)
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('First name'), { target: { value: 'Ada' } })

    fireEvent.submit(screen.getByRole('button', { name: 'Create policy' }).closest('form')!)

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('Policy creation may have succeeded')
    expect(alert).toHaveTextContent('do not resubmit')
    expect(alert).not.toHaveTextContent('try again')
    expect(screen.getByLabelText('First name')).toHaveValue('Ada')
    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(screen.getByRole('button', { name: 'Create policy' })).toBeEnabled()
  })

  it.each(['renewal', 'cancellation'])('requires GET recovery after %s returns HTTP 500', async operation => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
      .mockResolvedValueOnce(new Response(JSON.stringify(quote)))
      .mockResolvedValueOnce(new Response('{}', { status: 500 }))
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.submit(screen.getByLabelText('Policy reference').closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })
    fireEvent.submit(screen.getByRole('heading', { name: 'Cancellation estimate' }).closest('form')!)
    await screen.findByText(`Calculated for ${quote.date}`)

    if (operation === 'renewal') fireEvent.click(screen.getByRole('button', { name: 'Record automatic renewal' }))
    else {
      fireEvent.click(screen.getByRole('button', { name: 'Cancel policy term' }))
      fireEvent.click(screen.getByRole('button', { name: 'Confirm cancellation' }))
    }

    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('outcome is uncertain'))
    expect(screen.queryByText(`Calculated for ${quote.date}`)).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Record automatic renewal' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Cancel policy term' })).toBeDisabled()
    expect(fetchMock).toHaveBeenCalledTimes(3)
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.getByRole('button', { name: 'Record automatic renewal' })).toBeEnabled())
    expect(fetchMock.mock.calls.filter(call => call[1]?.method === 'POST')).toHaveLength(1)
    expect(fetchMock).toHaveBeenCalledTimes(4)
  })

  it('renders retrieved history and refreshes after a conflict', async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(policy), { status: 200 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ detail: 'Changed elsewhere.' }), { status: 409 }))
      .mockResolvedValueOnce(new Response(JSON.stringify(policy), { status: 200 }))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)

    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.submit(screen.getByLabelText('Policy reference').closest('form')!)
    expect(await screen.findByRole('heading', { name: policy.reference })).toBeVisible()
    fireEvent.submit(screen.getByRole('heading', { name: 'Automatic renewal' }).closest('form')!)

    expect(await screen.findByRole('alert')).toHaveTextContent('Changed elsewhere.')
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(3))
  })

  it('preserves a business conflict when refresh fails and recovers with GET only', async () => {
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
      .mockResolvedValueOnce(new Response(JSON.stringify({ detail: 'Outside the renewal window.' }), { status: 409 }))
      .mockResolvedValueOnce(new Response('{}', { status: 503 }))
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.submit(screen.getByLabelText('Policy reference').closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })
    fireEvent.click(screen.getByRole('button', { name: 'Record automatic renewal' }))
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Refresh failed'))
    expect(screen.getByRole('alert')).toHaveTextContent('Outside the renewal window.')
    expect(screen.getByRole('alert')).not.toHaveTextContent('latest state')
    expect(screen.getByRole('button', { name: 'Record automatic renewal' })).toBeDisabled()
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.getByRole('button', { name: 'Record automatic renewal' })).toBeEnabled())
    expect(fetchMock.mock.calls.filter(call => call[1]?.method === 'POST')).toHaveLength(1)
  })

  it.each(['renewal', 'cancellation'])('retains confirmed %s after a failed GET', async operation => {
    const successor = { ...term, id: 'new-term', predecessorTermId: term.id, startDate: '2026-10-13', endDate: '2027-10-12' }
    const result = operation === 'renewal' ? successor : quote
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
      .mockResolvedValueOnce(new Response(JSON.stringify(result), { status: 201 }))
      .mockResolvedValueOnce(new Response('{}', { status: 503 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ ...policy, terms: operation === 'renewal'
        ? [term, successor] : [{ ...term, state: 'Cancelled', cancellation: quote }] })))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.submit(screen.getByLabelText('Policy reference').closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })
    if (operation === 'renewal') fireEvent.click(screen.getByRole('button', { name: 'Record automatic renewal' }))
    else {
      fireEvent.click(screen.getByRole('button', { name: 'Cancel policy term' }))
      fireEvent.click(screen.getByRole('button', { name: 'Confirm cancellation' }))
    }
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Refresh failed'))
    expect(screen.getByText(operation === 'renewal'
      ? 'Renewal recorded and the new term is shown in the history.'
      : 'Cancellation recorded using a fresh calculation for today.')).toBeVisible()
    if (operation === 'renewal') {
      expect(screen.getAllByText('Term 2')).toHaveLength(1)
      expect(screen.getAllByRole('button', { name: 'Record automatic renewal' }).every(button => button.hasAttribute('disabled'))).toBe(true)
    } else expect(screen.queryByRole('button', { name: 'Cancel policy term' })).not.toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.queryByRole('button', { name: 'Refresh policy' })).not.toBeInTheDocument())
    if (operation === 'renewal') expect(screen.getAllByText('Term 2')).toHaveLength(1)
    expect(fetchMock.mock.calls.filter(call => call[1]?.method === 'POST')).toHaveLength(1)
  })

  it('blocks duplicate requests and policy navigation while a mutation outcome is unknown', async () => {
    let rejectMutation!: (reason: unknown) => void
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
      .mockImplementationOnce(() => new Promise((_resolve, reject) => { rejectMutation = reject }))
      .mockResolvedValueOnce(new Response(JSON.stringify(policy)))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.submit(screen.getByLabelText('Policy reference').closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })
    const renewalForm = screen.getByRole('heading', { name: 'Automatic renewal' }).closest('form')!
    fireEvent.submit(renewalForm)
    fireEvent.submit(renewalForm)
    expect(screen.getByLabelText('Policy reference')).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Sell another policy' })).toBeDisabled()
    rejectMutation(new TypeError('Network failure'))
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('outcome is uncertain'))
    expect(screen.getByRole('button', { name: 'Record automatic renewal' })).toBeDisabled()
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.getByRole('button', { name: 'Record automatic renewal' })).toBeEnabled())
    expect(fetchMock.mock.calls.filter(call => call[1]?.method === 'POST')).toHaveLength(1)
  })

  it('submits optional address lines without Bedrooms and normalizes empty City', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify(policy), { status: 201 }))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    expect(screen.queryByLabelText('Bedrooms')).not.toBeInTheDocument()
    fireEvent.change(screen.getByLabelText('Address line 1'), { target: { value: '1 Road' } })
    fireEvent.change(screen.getByLabelText('Address line 3'), { target: { value: ' Third line ' } })
    fireEvent.change(screen.getByLabelText('Town or city'), { target: { value: ' ' } })
    fireEvent.submit(screen.getByRole('button', { name: 'Create policy' }).closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })
    const body = JSON.parse(fetchMock.mock.calls[0][1].body)
    expect(body.property.addressLine3).toBe('Third line')
    expect(body.property.city).toBeNull()
    expect(body.property.addressLine2).toBeNull()
    expect(body.property).not.toHaveProperty('bedrooms')
  })

  it('captures one to three policyholders and submits every holder', async () => {
    const threeHolderPolicy = {
      ...policy,
      terms: [{ ...term, policyholders: [
        { firstName: 'Ada', lastName: 'One', dateOfBirth: '1990-01-01' },
        { firstName: 'Grace', lastName: 'Two', dateOfBirth: '1988-02-02' },
        { firstName: 'Katherine', lastName: 'Three', dateOfBirth: '1986-03-03' },
      ] }],
    }
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify(threeHolderPolicy), { status: 201 }))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)

    fireEvent.click(screen.getByRole('button', { name: 'Add another policy holder' }))
    fireEvent.click(screen.getByRole('button', { name: 'Add another policy holder' }))
    expect(screen.getByRole('button', { name: 'Add another policy holder' })).toBeDisabled()
    screen.getAllByLabelText('First name').forEach((input, index) => fireEvent.change(input, { target: { value: ['Ada', 'Grace', 'Katherine'][index] } }))
    screen.getAllByLabelText('Last name').forEach((input, index) => fireEvent.change(input, { target: { value: ['One', 'Two', 'Three'][index] } }))
    screen.getAllByLabelText('Date of birth').forEach((input, index) => fireEvent.change(input, { target: { value: `199${index}-01-01` } }))

    fireEvent.submit(screen.getByRole('button', { name: 'Create policy' }).closest('form')!)
    await screen.findByRole('heading', { name: policy.reference })
    const body = JSON.parse(fetchMock.mock.calls[0][1].body)
    expect(body.policyholders).toHaveLength(3)
    expect(body.policyholders.map((holder: { firstName: string }) => holder.firstName)).toEqual(['Ada', 'Grace', 'Katherine'])
    expect(screen.getByRole('heading', { name: 'Policy holders (3)' })).toBeVisible()
    expect(screen.getByText('Katherine Three')).toBeVisible()
  })

  it('keeps birth dates aligned with the start date and prevents automatic renewal by cheque', () => {
    render(<App />)
    const start = screen.getByLabelText('Start date')
    const birthDate = screen.getByLabelText('Date of birth')
    const tomorrow = new Date(`${todayForTest()}T00:00:00Z`)
    tomorrow.setUTCDate(tomorrow.getUTCDate() + 1)
    const tomorrowText = tomorrow.toISOString().slice(0, 10)

    fireEvent.change(start, { target: { value: tomorrowText } })
    expect(birthDate).toHaveAttribute('max', `${tomorrow.getUTCFullYear() - 16}-${String(tomorrow.getUTCMonth() + 1).padStart(2, '0')}-${String(tomorrow.getUTCDate()).padStart(2, '0')}`)
    const payment = screen.getByLabelText('Payment method')
    expect(screen.getByRole('option', { name: 'Cheque' })).toBeDisabled()
    fireEvent.click(screen.getByLabelText('Manual'))
    expect(screen.getByRole('option', { name: 'Cheque' })).toBeEnabled()
    fireEvent.change(payment, { target: { value: 'Cheque' } })
    fireEvent.click(screen.getByLabelText('Automatic'))
    expect(payment).toHaveValue('Card')
    expect(screen.getByRole('option', { name: 'Cheque' })).toBeDisabled()
  })

  it('loads a stable demo scenario and shows renewal and holder facts', async () => {
    const demo = { ...policy, reference: 'POL-DEMO-AUTO-BTL', terms: [{ ...term, policyholders: [
      ...term.policyholders,
      { firstName: 'Grace', lastName: 'Example', dateOfBirth: '1985-12-09' },
      { firstName: 'Alan', lastName: 'Example', dateOfBirth: '1982-06-23' },
    ] }] }
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify(demo)))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)

    fireEvent.click(screen.getByText('Try a prepared policy'))
    fireEvent.click(screen.getByRole('button', { name: /Automatic · Buy to Let/ }))

    await screen.findByRole('heading', { name: demo.reference })
    expect(screen.getByText('Automatic', { selector: '.renewal-mode' })).toBeVisible()
    expect(screen.getByRole('heading', { name: 'Policy holders (3)' })).toBeVisible()
    expect(fetchMock).toHaveBeenCalledWith('/api/policies/POL-DEMO-AUTO-BTL', expect.anything())
  })

  it('creates a manual renewal without sending a payment method', async () => {
    const manualTerm = { ...term, autoRenew: false }
    const manualPolicy = { ...policy, reference: 'POL-DEMO-MANUAL-HH', terms: [manualTerm] }
    const successor = { ...manualTerm, id: 'manual-successor', predecessorTermId: manualTerm.id, startDate: '2026-10-13', endDate: '2027-10-12', payment: null, paymentState: 'NotRecorded' }
    const fetchMock = vi.fn()
      .mockResolvedValueOnce(new Response(JSON.stringify(manualPolicy)))
      .mockResolvedValueOnce(new Response(JSON.stringify(successor), { status: 201 }))
      .mockResolvedValueOnce(new Response(JSON.stringify({ ...manualPolicy, terms: [manualTerm, successor] })))
    vi.stubGlobal('fetch', fetchMock)
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: manualPolicy.reference } })
    fireEvent.click(screen.getByRole('button', { name: 'Find policy' }))

    await screen.findByRole('heading', { name: manualPolicy.reference })
    fireEvent.click(screen.getByRole('button', { name: 'Create unpaid renewal' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(3))
    expect(JSON.parse(fetchMock.mock.calls[1][1].body)).toEqual({ paymentMethod: null })
    expect(screen.getByText('NotRecorded')).toBeVisible()
  })

  it('prevents lifecycle actions that history or dates make unavailable', async () => {
    const successor = { ...term, id: 'successor', predecessorTermId: term.id, startDate: '2026-10-13', endDate: '2027-10-12', state: 'Scheduled' as const }
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ ...policy, terms: [term, successor] }))))
    render(<App />)
    fireEvent.change(screen.getByLabelText('Policy reference'), { target: { value: policy.reference } })
    fireEvent.click(screen.getByRole('button', { name: 'Find policy' }))

    await screen.findByRole('heading', { name: policy.reference })
    expect(screen.getByText('A successor term already exists.')).toBeVisible()
    expect(screen.getByText('A term with an active successor cannot be cancelled.')).toBeVisible()
    expect(screen.getAllByRole('button', { name: 'Record automatic renewal' }).every(button => button.hasAttribute('disabled'))).toBe(true)
    expect(screen.getAllByRole('button', { name: 'Cancel policy term' })[0]).toBeDisabled()
  })
})

function todayForTest() {
  return new Date().toISOString().slice(0, 10)
}
