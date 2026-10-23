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

  /** Shows a short confirmation; it is announced politely and goes away by itself. */
  show(message: string): void {
    const toast = { id: this.nextId++, message };
    this.list.update((toasts) => [...toasts, toast]);
    setTimeout(
      () => this.list.update((toasts) => toasts.filter((t) => t !== toast)),
      DURATION_MS,
    );
  }
}
