import { readFileSync, writeFileSync } from 'node:fs'
import { join } from 'node:path'
import { expect, test, type Page } from '@playwright/test'

const manifest = JSON.parse(readFileSync(process.env.UINSURE_E2E_MANIFEST!, 'utf8')) as {
  today: string
  fixtures: Record<string, { reference: string; termId: string; startDate: string; endDate: string }>
}

async function find(page: Page, reference: string) {
  await page.goto('/')
  await page.getByLabel('Policy reference').fill(reference)
  await page.getByRole('button', { name: 'Find policy' }).click()
  await expect(page.getByRole('heading', { name: reference, exact: true })).toBeVisible()
}

function receipt(browser: string, kind: string, reference: string, refund: number | null = null) {
  writeFileSync(join(process.env.UINSURE_E2E_RECEIPTS!, `${browser}-${kind}.json`), JSON.stringify({ reference, kind, refund }))
}

test('real sale persists all address lines and is retrieved after reload', async ({ page }, info) => {
  await page.goto('/')
  await page.getByLabel('First name').fill('Ada')
  await page.getByLabel('Last name').fill('Example')
  await page.getByLabel('Date of birth').fill('1990-01-01')
  await page.getByLabel('Start date').fill(manifest.today)
  await page.getByLabel('Address line 1').fill('1 Synthetic Road')
  await page.getByLabel('Address line 2').fill('Second line')
  await page.getByLabel('Address line 3').fill('Third line')
  await page.getByLabel('Postcode').fill('M1 1AA')
  const response = page.waitForResponse(r => r.request().method() === 'POST' && r.url().endsWith('/api/policies'))
  await page.getByRole('button', { name: 'Create policy' }).click()
  const created = await response
  expect(created.status()).toBe(201)
  const { reference } = await created.json()
  await find(page, reference)
  await expect(page.getByText('1 Synthetic Road, Second line, Third line, M1 1AA')).toBeVisible()
  await expect(page.getByText('Term 1')).toBeVisible()
  receipt(info.project.name, 'sale', reference)
  await page.screenshot({ path: `test-results/fullstack-${info.project.name}.png`, fullPage: true })
})

test('real quote and cancellation persist a refund', async ({ page }, info) => {
  const fixture = manifest.fixtures[`${info.project.name}-day15`]
  await find(page, fixture.reference)
  await page.getByLabel('Quote date').fill(manifest.today)
  const quoted = page.waitForResponse(r => r.url().includes('/cancellation-quote?'))
  await page.getByRole('button', { name: 'Get quote' }).click()
  const result = await (await quoted).json()
  expect(result.reason).toBe('ProRata')
  expect(result.usedDays).toBe(14)
  await page.getByRole('button', { name: 'Cancel policy term' }).click()
  const cancelled = page.waitForResponse(r => r.url().endsWith('/cancellations'))
  await page.getByRole('button', { name: 'Confirm cancellation' }).click()
  expect((await cancelled).status()).toBe(201)
  await expect(page.getByText(`Cancelled ${manifest.today}`, { exact: false })).toBeVisible()
  await find(page, fixture.reference)
  await expect(page.getByRole('button', { name: 'Cancel policy term' })).toHaveCount(0)
  receipt(info.project.name, 'cancel', fixture.reference, result.refundAmount)
})

for (const kind of ['paid', 'manual']) {
  test(`real ${kind} renewal retains predecessor history`, async ({ page }, info) => {
    const fixture = manifest.fixtures[`${info.project.name}-${kind}`]
    await find(page, fixture.reference)
    if (kind === 'paid') await page.getByLabel('Payment method').selectOption('DirectDebit')
    const renewed = page.waitForResponse(r => r.url().endsWith('/renewals'))
    await page.getByRole('button', { name: 'Renew term' }).click()
    const response = await renewed
    expect(response.status()).toBe(201)
    const successor = await response.json()
    expect(successor.paymentState).toBe(kind === 'paid' ? 'Recorded' : 'NotRecorded')
    expect(successor.predecessorTermId).toBe(fixture.termId)
    await find(page, fixture.reference)
    await expect(page.getByText('Term 2')).toBeVisible()
    await expect(page.getByText(fixture.startDate, { exact: false }).first()).toBeVisible()
    receipt(info.project.name, kind, fixture.reference)
  })
}
