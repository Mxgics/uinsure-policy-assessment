import '@testing-library/jest-dom/vitest'
import { vi } from 'vitest'

// Match the historical contract fixtures without changing asynchronous timers.
vi.useFakeTimers({ toFake: ['Date'] })
vi.setSystemTime(new Date('2026-09-28T09:00:00Z'))

// jsdom has no top-layer implementation. Actual focus/inertness is tested in Chromium.
HTMLDialogElement.prototype.showModal = function () { this.setAttribute('open', '') }
HTMLDialogElement.prototype.close = function () {
  this.removeAttribute('open')
  this.dispatchEvent(new Event('close'))
}
