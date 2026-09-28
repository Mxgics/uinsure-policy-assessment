import '@testing-library/jest-dom/vitest'

// jsdom has no top-layer implementation. Actual focus/inertness is tested in Chromium.
HTMLDialogElement.prototype.showModal = function () { this.setAttribute('open', '') }
HTMLDialogElement.prototype.close = function () {
  this.removeAttribute('open')
  this.dispatchEvent(new Event('close'))
}
