import type { CancellationResult, Policy, PolicyTerm, ProblemDetails, SellPolicyInput } from './types'

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails,
  ) {
    super(problem.detail ?? problem.title ?? `Request failed (${status})`)
  }
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: init?.body ? { 'content-type': 'application/json', ...init.headers } : init?.headers,
  })
  const body = await response.json().catch(() => ({}))
  if (!response.ok) throw new ApiError(response.status, body as ProblemDetails)
  return body as T
}

export const policyApi = {
  sell: (input: SellPolicyInput) => request<Policy>('/api/policies', { method: 'POST', body: JSON.stringify(input) }),
  get: (reference: string) => request<Policy>(`/api/policies/${encodeURIComponent(reference.trim())}`),
  quote: (reference: string, termId: string, date: string) =>
    request<CancellationResult>(`/api/policies/${encodeURIComponent(reference)}/terms/${termId}/cancellation-quote?date=${date}`),
  cancel: (reference: string, termId: string) =>
    request<CancellationResult>(`/api/policies/${encodeURIComponent(reference)}/terms/${termId}/cancellations`, { method: 'POST' }),
  renew: (reference: string, termId: string, paymentMethod: string | null) =>
    request<PolicyTerm>(`/api/policies/${encodeURIComponent(reference)}/terms/${termId}/renewals`, {
      method: 'POST',
      body: JSON.stringify({ paymentMethod }),
    }),
}
