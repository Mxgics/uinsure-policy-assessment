import { expect, test, type Page } from '@playwright/test'
import { policy, quote, term } from '../src/test-fixtures'

test.beforeEach(async ({ page }) => {
  // Match the historical contracts; the real full-stack suite retains UTC today.
  await page.clock.setFixedTime('2026-09-28T09:00:00Z')
})

async function json(page: Page, url: string, body: unknown, status = 200) {
  await page.route(url, route => route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) }))
}

test('long policy references fit the viewport without hiding actions', async ({ page }, info) => {
  const reference = 'POL-1234567890123456789012345678'
  await json(page, `**/api/policies/${reference}`, { ...policy, reference })
  await page.goto('/')
  await page.getByLabel('Policy reference').fill(reference)
  await page.getByRole('button', { name: 'Find policy' }).click()
  await expect(page.getByRole('heading', { name: reference })).toBeVisible()
  await page.screenshot({ path: `test-results/long-reference-${info.project.name}.png`, fullPage: true })
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(page.viewportSize()!.width)
  await page.getByRole('button', { name: 'Record automatic renewal' }).click({ trial: true })
})

test('prepared policy disclosure is keyboard accessible and collapses after loading', async ({ page }) => {
  const reference = 'POL-DEMO-AUTO-HH'
  await json(page, `**/api/policies/${reference}`, { ...policy, reference })
  await page.goto('/')
  const disclosure = page.locator('details.demo-panel')
  const summary = disclosure.locator('summary')
  await summary.focus()
  await page.keyboard.press('Enter')
  await expect(disclosure).toHaveAttribute('open', '')
  await page.getByRole('button', { name: /Automatic · Household/ }).click()
  await expect(page.getByRole('heading', { name: reference })).toBeVisible()
  await expect(disclosure).not.toHaveAttribute('open', '')
})

test('sell three holders and display the complete policy', async ({ page }, testInfo) => {
  const holders = [
    { firstName: 'Ada', lastName: 'Lovelace', dateOfBirth: '1990-01-01' },
    { firstName: 'Grace', lastName: 'Hopper', dateOfBirth: '1988-02-02' },
    { firstName: 'Katherine', lastName: 'Johnson', dateOfBirth: '1986-03-03' },
  ]
  await json(page, '**/api/policies', { ...policy, terms: [{ ...term, policyholders: holders }] }, 201)
  await page.goto('/')
  await page.getByRole('button', { name: 'Add another policy holder' }).click()
  await page.getByRole('button', { name: 'Add another policy holder' }).click()
  for (let index = 0; index < holders.length; index++) {
    await page.getByLabel('First name').nth(index).fill(holders[index].firstName)
    await page.getByLabel('Last name').nth(index).fill(holders[index].lastName)
    await page.getByLabel('Date of birth').nth(index).fill(holders[index].dateOfBirth)
  }
  await page.getByLabel('Address line 1').fill('1 Test Road')
  await page.getByLabel('Town or city').fill('Manchester')
  await page.getByLabel('Postcode').fill('M1 1AA')
  await page.getByRole('button', { name: 'Create policy' }).click()

  await expect(page.getByRole('heading', { name: policy.reference })).toBeVisible()
  await expect(page.getByText('£365.00')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Policy holders (3)' })).toBeVisible()
  await expect(page.getByText('Katherine Johnson')).toBeVisible()
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
  await expect(page.getByRole('button', { name: 'Keep term' })).toBeFocused()
  await page.getByRole('button', { name: 'Confirm cancellation' }).click()

  await expect(page.getByText(/Cancelled 2026-10-15/)).toBeVisible()
  await expect(page.locator(`#term-${term.id}`)).toBeFocused()
})

test('cancellation modal contains focus, dismisses safely and has readable destructive text', async ({ page }) => {
  await json(page, `**/api/policies/${policy.reference}`, policy)
  let posts = 0
  page.on('request', request => { if (request.method() === 'POST') posts++ })
  await page.goto('/')
  await page.getByLabel('Policy reference').fill(policy.reference)
  await page.getByRole('button', { name: 'Find policy' }).click()
  const trigger = page.getByRole('button', { name: 'Cancel policy term' })
  await trigger.click()
  const keep = page.getByRole('button', { name: 'Keep term' })
  const confirm = page.getByRole('button', { name: 'Confirm cancellation' })
  await expect(keep).toBeFocused()
  await page.keyboard.press('Shift+Tab')
  await expect(confirm).toBeFocused()
  await page.keyboard.press('Tab')
  await expect(keep).toBeFocused()
  await page.getByLabel('Policy reference').evaluate(element => (element as HTMLElement).focus())
  await expect(keep).toBeFocused()
  const contrast = await confirm.evaluate(element => {
    const style = getComputedStyle(element)
    const luminance = (color: string) => {
      const channels = color.match(/[\d.]+/g)!.slice(0, 3).map(Number).map(v => {
        const s = v / 255
        return s <= 0.04045 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4
      })
      return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722
    }
    const a = luminance(style.color), b = luminance(style.backgroundColor)
    return (Math.max(a, b) + 0.05) / (Math.min(a, b) + 0.05)
  })
  expect(contrast).toBeGreaterThanOrEqual(4.5)
  await page.keyboard.press('Escape')
  await expect(page.getByRole('dialog')).toHaveCount(0)
  await expect(trigger).toBeFocused()
  await trigger.click()
  await keep.click()
  await expect(trigger).toBeFocused()
  expect(posts).toBe(0)
})

test('renew and display successor history', async ({ page }) => {
  const successor = {
    ...term,
    id: '22222222-2222-2222-2222-222222222222',
    predecessorTermId: term.id,
    startDate: '2026-10-13',
    endDate: '2027-10-12',
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
  await page.getByRole('button', { name: 'Record automatic renewal' }).click()

  await expect(page.getByText('Term 2')).toBeVisible()
  await expect(page.getByText('2026-10-13 → 2027-10-12')).toBeVisible()
})
