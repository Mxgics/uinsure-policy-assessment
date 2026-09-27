import type { Policy, PolicyTerm } from './types'

export const term: PolicyTerm = {
  id: '11111111-1111-1111-1111-111111111111',
  predecessorTermId: null,
  startDate: '2026-10-01',
  endDate: '2027-09-30',
  premium: 365,
  hasClaims: false,
  autoRenew: true,
  state: 'Current',
  paymentState: 'Recorded',
  policyholders: [{ firstName: 'Ada', lastName: 'Lovelace', dateOfBirth: '1990-01-01' }],
  property: { addressLine1: '1 Test Road', addressLine2: null, city: 'Manchester', postcode: 'M1 1AA', bedrooms: 3 },
  payment: { reference: 'PAY-TEST', method: 'Card', amount: 365 },
  cancellation: null,
}

export const policy: Policy = { reference: 'POL-DEMO123', type: 'Household', terms: [term] }

export const quote = {
  date: '2026-10-15',
  refundAmount: 351,
  retainedPremium: 14,
  currency: 'GBP',
  method: 'Card' as const,
  reason: 'ProRata',
  totalDays: 365,
  usedDays: 14,
  unusedDays: 351,
  recordedAtUtc: null,
  refund: null,
}
