export type PaymentMethod = 'Card' | 'DirectDebit' | 'Cheque'

export interface ProblemDetails {
  title?: string
  detail?: string
  code?: string
  errors?: Record<string, string[]>
}

export interface CancellationResult {
  date: string
  refundAmount: number
  retainedPremium: number
  currency: string
  method: PaymentMethod | null
  reason: string
  totalDays: number
  usedDays: number
  unusedDays: number
  recordedAtUtc: string | null
  refund: { reference: string; amount: number; method: PaymentMethod } | null
}

export interface PolicyTerm {
  id: string
  predecessorTermId: string | null
  startDate: string
  endDate: string
  premium: number
  hasClaims: boolean
  autoRenew: boolean
  state: 'Scheduled' | 'Current' | 'Expired' | 'Cancelled'
  paymentState: 'Recorded' | 'NotRecorded'
  policyholders: Array<{ firstName: string; lastName: string; dateOfBirth: string }>
  property: { addressLine1: string; addressLine2: string | null; addressLine3: string | null; city: string | null; postcode: string }
  payment: { reference: string; method: PaymentMethod; amount: number } | null
  cancellation: CancellationResult | null
}

export interface Policy {
  reference: string
  type: 'Household' | 'BuyToLet'
  terms: PolicyTerm[]
}

export interface SellPolicyInput {
  type: Policy['type']
  startDate: string
  premium: number
  hasClaims: boolean
  autoRenew: boolean
  policyholders: Array<{ firstName: string; lastName: string; dateOfBirth: string }>
  property: PolicyTerm['property']
  paymentMethod: PaymentMethod
}
