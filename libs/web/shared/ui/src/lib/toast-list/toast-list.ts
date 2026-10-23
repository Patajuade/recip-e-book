import { Component, inject } from '@angular/core';
import { Toaster } from './toaster';

/**
 * Renders the toasts from `Toaster`; placed once in the app layout. It is the live region,
 * so it must be rendered before any toast. `aria-atomic="false"` makes screen readers read
 * only the new toast, not every visible one.
 */
@Component({
  selector: 'lib-toast-list',
  templateUrl: './toast-list.html',
  styleUrl: './toast-list.css',
  host: {
    role: 'status',
    'aria-atomic': 'false',
    class:
      'pointer-events-none fixed inset-x-0 bottom-4 flex flex-col items-center gap-2 px-4 sm:items-end',
  },
})
export class ToastList {
  protected readonly toaster = inject(Toaster);
}
