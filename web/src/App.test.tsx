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

    if (operation === 'renewal') fireEvent.click(screen.getByRole('button', { name: 'Renew term' }))
    else {
      fireEvent.click(screen.getByRole('button', { name: 'Cancel policy term' }))
      fireEvent.click(screen.getByRole('button', { name: 'Confirm cancellation' }))
    }

    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('outcome is uncertain'))
    expect(screen.queryByText(`Calculated for ${quote.date}`)).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Renew term' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Cancel policy term' })).toBeDisabled()
    expect(fetchMock).toHaveBeenCalledTimes(3)
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.getByRole('button', { name: 'Renew term' })).toBeEnabled())
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
    fireEvent.submit(screen.getByRole('heading', { name: 'Renew this term' }).closest('form')!)

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
    fireEvent.click(screen.getByRole('button', { name: 'Renew term' }))
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('Refresh failed'))
    expect(screen.getByRole('alert')).toHaveTextContent('Outside the renewal window.')
    expect(screen.getByRole('alert')).not.toHaveTextContent('latest state')
    expect(screen.getByRole('button', { name: 'Renew term' })).toBeDisabled()
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.getByRole('button', { name: 'Renew term' })).toBeEnabled())
    expect(fetchMock.mock.calls.filter(call => call[1]?.method === 'POST')).toHaveLength(1)
  })

  it.each(['renewal', 'cancellation'])('retains confirmed %s after a failed GET', async operation => {
    const successor = { ...term, id: 'new-term', predecessorTermId: term.id, startDate: '2027-10-01' }
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
    if (operation === 'renewal') fireEvent.click(screen.getByRole('button', { name: 'Renew term' }))
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
      expect(screen.getAllByRole('button', { name: 'Renew term' }).every(button => button.hasAttribute('disabled'))).toBe(true)
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
    const renewalForm = screen.getByRole('heading', { name: 'Renew this term' }).closest('form')!
    fireEvent.submit(renewalForm)
    fireEvent.submit(renewalForm)
    expect(screen.getByLabelText('Policy reference')).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Sell another policy' })).toBeDisabled()
    rejectMutation(new TypeError('Network failure'))
    await waitFor(() => expect(screen.getByRole('alert')).toHaveTextContent('outcome is uncertain'))
    expect(screen.getByRole('button', { name: 'Renew term' })).toBeDisabled()
    fireEvent.click(screen.getByRole('button', { name: 'Refresh policy' }))
    await waitFor(() => expect(screen.getByRole('button', { name: 'Renew term' })).toBeEnabled())
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
})
