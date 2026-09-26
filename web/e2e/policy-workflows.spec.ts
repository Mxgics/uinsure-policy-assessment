import { expect, test, type Page } from '@playwright/test'
import { policy, quote, term } from '../src/test-fixtures'

async function json(page: Page, url: string, body: unknown, status = 200) {
  await page.route(url, route => route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) }))
}

test('sell and display a policy', async ({ page }, testInfo) => {
  await json(page, '**/api/policies', policy, 201)
  await page.goto('/')
  await page.getByLabel('First name').fill('Ada')
  await page.getByLabel('Last name').fill('Lovelace')
  await page.getByLabel('Date of birth').fill('1990-01-01')
  await page.getByLabel('Address line 1').fill('1 Test Road')
  await page.getByLabel('Town or city').fill('Manchester')
  await page.getByLabel('Postcode').fill('M1 1AA')
  await page.getByRole('button', { name: 'Create policy' }).click()

  await expect(page.getByRole('heading', { name: policy.reference })).toBeVisible()
  await expect(page.getByText('£365.00')).toBeVisible()
  await page.screenshot({ path: `test-results/ui-${testInfo.project.name}.png`, fullPage: true })
})

test('find, quote and confirm cancellation', async ({ page }) => {
  let getCount = 0
  await page.route(`**/api/policies/${policy.reference}`, route => {
    getCount++
    const body = getCount === 1 ? policy : {
      ...policy,
      terms: [{ ...term, state: 'Cancelled', cancellation: { ...quote, recordedAtUtc: '2026-10-15T09:00:00Z' } }],
    }
    return route.fulfill({ contentType: 'application/json', body: JSON.stringify(body) })
  })
  await json(page, '**/cancellation-quote?date=*', quote)
  await json(page, '**/cancellations', quote, 201)
  await page.goto('/')
  await page.getByLabel('Policy reference').fill(policy.reference)
  await page.getByRole('button', { name: 'Find policy' }).click()
  await page.getByRole('button', { name: 'Get quote' }).click()
  await expect(page.getByLabel('Cancellation quote result')).toContainText('£351.00 refund')
  await page.getByRole('button', { name: 'Cancel policy term' }).click()
  await expect(page.getByRole('dialog')).toContainText('calculate today’s result again')
  await expect(page.getByRole('button', { name: 'Confirm cancellation' })).toBeFocused()
  await page.getByRole('button', { name: 'Confirm cancellation' }).click()

  await expect(page.getByText(/Cancelled 2026-10-15/)).toBeVisible()
})

test('renew and display successor history', async ({ page }) => {
  const successor = {
    ...term,
    id: '22222222-2222-2222-2222-222222222222',
    predecessorTermId: term.id,
    startDate: '2027-10-01',
    endDate: '2028-09-30',
    state: 'Scheduled',
  }
  let getCount = 0
  await page.route(`**/api/policies/${policy.reference}`, route => {
    getCount++
    const body = getCount === 1 ? policy : { ...policy, terms: [term, successor] }
    return route.fulfill({ contentType: 'application/json', body: JSON.stringify(body) })
  })
  await json(page, '**/renewals', successor, 201)
  await page.goto('/')
  await page.getByLabel('Policy reference').fill(policy.reference)
  await page.getByRole('button', { name: 'Find policy' }).click()
  await page.getByRole('button', { name: 'Renew term' }).click()

  await expect(page.getByText('Term 2')).toBeVisible()
  await expect(page.getByText('2027-10-01 → 2028-09-30')).toBeVisible()
})
