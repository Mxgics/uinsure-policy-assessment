import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiError, policyApi } from './api'

afterEach(() => vi.unstubAllGlobals())

describe('API response boundary', () => {
  it.each([
    ['invalid JSON', new Response('{not-json', { status: 200 })],
    ['an empty body', new Response(null, { status: 200 })],
    ['a structurally unusable body', new Response(JSON.stringify({ reference: 'POL-ONLY' }), { status: 200 })],
  ])('rejects a successful response containing %s', async (_description, response) => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(response))

    await expect(policyApi.get('POL-TEST')).rejects.toThrow('unusable response')
  })

  it('turns a malformed error body into a safe API error', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('<html>failure</html>', { status: 503 })))

    await expect(policyApi.get('POL-TEST')).rejects.toMatchObject<ApiError>({
      status: 503,
      message: 'Request failed (503)',
      problem: { title: 'Request failed (503)' },
    })
  })
})
