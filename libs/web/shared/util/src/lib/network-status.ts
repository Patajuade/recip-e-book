import { DOCUMENT, inject, Service, signal } from '@angular/core';

/**
 * Whether the browser thinks it is online (`navigator.onLine` and its `online` / `offline`
 * events). Only a hint, as MDN warns: show it, don't disable features with it.
 */
@Service()
export class NetworkStatus {
  private readonly window = inject(DOCUMENT).defaultView;
  private readonly state = signal(this.window?.navigator.onLine ?? true);
  readonly online = this.state.asReadonly();

  constructor() {
    this.window?.addEventListener('online', () => this.state.set(true));
    this.window?.addEventListener('offline', () => this.state.set(false));
  }
}
