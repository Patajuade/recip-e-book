import { Service, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
}

const DURATION_MS = 5000;

@Service()
export class Toaster {
  private nextId = 0;
  private readonly list = signal<Toast[]>([]);
  readonly toasts = this.list.asReadonly();

  /**
   * Shows a short message; it is announced politely and goes away by itself.
   * Ignored while the same message is already shown (e.g. several requests failing at once).
   */
  show(message: string): void {
    if (this.list().some((toast) => toast.message === message)) {
      return;
    }
    const toast = { id: this.nextId++, message };
    this.list.update((toasts) => [...toasts, toast]);
    setTimeout(
      () => this.list.update((toasts) => toasts.filter((t) => t !== toast)),
      DURATION_MS,
    );
  }
}
