import type { CancellationResult, Policy, PolicyTerm, ProblemDetails, SellPolicyInput } from './types'

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails,
  ) {
    super(problem.detail ?? problem.title ?? `Request failed (${status})`)
  }
}

export class ResponseFormatError extends Error {
  constructor() {
    super('The server returned an unusable response.')
  }
}

type JsonObject = Record<string, unknown>
type Validator<T> = (value: unknown) => value is T

const isObject = (value: unknown): value is JsonObject =>
  typeof value === 'object' && value !== null && !Array.isArray(value)
const isString = (value: unknown): value is string => typeof value === 'string'
const isBoolean = (value: unknown): value is boolean => typeof value === 'boolean'
const isNullableString = (value: unknown): value is string | null => value === null || isString(value)
const isMoney = (value: unknown): value is number =>
  typeof value === 'number' && Number.isFinite(value) && Number.isSafeInteger(Math.round(value * 100))
const isPaymentMethod = (value: unknown): value is 'Card' | 'DirectDebit' | 'Cheque' =>
  value === 'Card' || value === 'DirectDebit' || value === 'Cheque'

const isProblemDetails = (value: unknown): value is ProblemDetails => {
  if (!isObject(value)) return false
  if (value.title !== undefined && !isString(value.title)) return false
  if (value.detail !== undefined && !isString(value.detail)) return false
  if (value.code !== undefined && !isString(value.code)) return false
  return value.errors === undefined || (isObject(value.errors) && Object.values(value.errors)
    .every(messages => Array.isArray(messages) && messages.every(isString)))
}

const isProperty = (value: unknown): value is PolicyTerm['property'] => isObject(value) &&
  isString(value.addressLine1) && isNullableString(value.addressLine2) && isNullableString(value.addressLine3) &&
  isNullableString(value.city) && isString(value.postcode)

const isCancellationResult = (value: unknown): value is CancellationResult => isObject(value) &&
  isString(value.date) && isMoney(value.refundAmount) && isMoney(value.retainedPremium) &&
  isString(value.currency) && (value.method === null || isPaymentMethod(value.method)) &&
  isString(value.reason) && Number.isInteger(value.totalDays) && Number.isInteger(value.usedDays) &&
  Number.isInteger(value.unusedDays) && (value.recordedAtUtc === null || isString(value.recordedAtUtc)) &&
  (value.refund === null || (isObject(value.refund) && isString(value.refund.reference) &&
    isMoney(value.refund.amount) && isPaymentMethod(value.refund.method)))

const isPolicyTerm = (value: unknown): value is PolicyTerm => isObject(value) &&
  isString(value.id) && isNullableString(value.predecessorTermId) && isString(value.startDate) &&
  isString(value.endDate) && isMoney(value.premium) && isBoolean(value.hasClaims) && isBoolean(value.autoRenew) &&
  ['Scheduled', 'Current', 'Expired', 'Cancelled'].includes(String(value.state)) &&
  ['Recorded', 'NotRecorded'].includes(String(value.paymentState)) && Array.isArray(value.policyholders) &&
  value.policyholders.every(holder => isObject(holder) && isString(holder.firstName) &&
    isString(holder.lastName) && isString(holder.dateOfBirth)) && isProperty(value.property) &&
  (value.payment === null || (isObject(value.payment) && isString(value.payment.reference) &&
    isPaymentMethod(value.payment.method) && isMoney(value.payment.amount))) &&
  (value.cancellation === null || isCancellationResult(value.cancellation))

const isPolicy = (value: unknown): value is Policy => isObject(value) && isString(value.reference) &&
  (value.type === 'Household' || value.type === 'BuyToLet') && Array.isArray(value.terms) &&
  value.terms.every(isPolicyTerm)

async function request<T>(url: string, validator: Validator<T>, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    ...init,
    headers: init?.body ? { 'content-type': 'application/json', ...init.headers } : init?.headers,
  })
  const text = await response.text()
  let body: unknown
  try {
    body = text ? JSON.parse(text) : undefined
  } catch {
    body = undefined
  }
  if (!response.ok) {
    const problem = isProblemDetails(body) ? body : { title: `Request failed (${response.status})` }
    throw new ApiError(response.status, problem)
  }
  if (!validator(body)) throw new ResponseFormatError()
  return body
}

export const policyApi = {
  sell: (input: SellPolicyInput) => request('/api/policies', isPolicy, { method: 'POST', body: JSON.stringify(input) }),
  get: (reference: string) => request(`/api/policies/${encodeURIComponent(reference.trim())}`, isPolicy),
  quote: (reference: string, termId: string, date: string) =>
    request(`/api/policies/${encodeURIComponent(reference)}/terms/${termId}/cancellation-quote?date=${date}`, isCancellationResult),
  cancel: (reference: string, termId: string) =>
    request(`/api/policies/${encodeURIComponent(reference)}/terms/${termId}/cancellations`, isCancellationResult, { method: 'POST' }),
  renew: (reference: string, termId: string, paymentMethod: string | null) =>
    request(`/api/policies/${encodeURIComponent(reference)}/terms/${termId}/renewals`, isPolicyTerm, {
      method: 'POST',
      body: JSON.stringify({ paymentMethod }),
    }),
}
