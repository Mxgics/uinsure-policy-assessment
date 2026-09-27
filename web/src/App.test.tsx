import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { App } from './App'
import { policy } from './test-fixtures'

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

    expect(await screen.findByRole('alert')).toHaveTextContent('Renewal was not applied because the policy changed')
    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(3))
  })
})
