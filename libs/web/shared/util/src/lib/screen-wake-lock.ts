import {
  DestroyRef,
  DOCUMENT,
  effect,
  inject,
  Service,
  signal,
} from '@angular/core';

/**
 * Keeps the screen on while `enabled` is true (Screen Wake Lock API). Provide it in the
 * component that needs it: the lock is released when that component is destroyed.
 */
@Service({ autoProvided: false })
export class ScreenWakeLock {
  private readonly document = inject(DOCUMENT);

  readonly supported = 'wakeLock' in navigator;
  readonly enabled = signal(true);
  /** False when the browser refused or released the lock (hidden tab, power saving…). */
  readonly active = signal(false);
  /** The last request was refused (e.g. power saving mode). */
  readonly failed = signal(false);

  private sentinel: WakeLockSentinel | null = null;

  constructor() {
    effect(() => {
      if (this.enabled()) {
        void this.acquire();
      } else {
        void this.sentinel?.release();
      }
    });

    // The lock is released when the page is hidden: take it again when it's back (MDN).
    const onVisibilityChange = () => {
      if (this.enabled() && this.document.visibilityState === 'visible') {
        void this.acquire();
      }
    };
    this.document.addEventListener('visibilitychange', onVisibilityChange);
    inject(DestroyRef).onDestroy(() => {
      this.document.removeEventListener('visibilitychange', onVisibilityChange);
      this.enabled.set(false);
      void this.sentinel?.release();
    });
  }

  private async acquire(): Promise<void> {
    if (!this.supported || this.sentinel) {
      return;
    }
    try {
      const sentinel = await navigator.wakeLock.request('screen');
      if (!this.enabled()) {
        // Turned off while the request was pending.
        await sentinel.release();
        return;
      }
      this.sentinel = sentinel;
      this.active.set(true);
      this.failed.set(false);
      sentinel.addEventListener('release', () => {
        this.sentinel = null;
        this.active.set(false);
      });
    } catch {
      this.active.set(false);
      this.failed.set(true);
    }
  }
}
