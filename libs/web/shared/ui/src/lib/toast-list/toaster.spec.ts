import { TestBed } from '@angular/core/testing';
import { Toaster } from './toaster';

describe('Toaster', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('removes a toast after 5 seconds', () => {
    const toaster = TestBed.inject(Toaster);

    toaster.show('Saved');
    vi.advanceTimersByTime(4999);
    expect(toaster.toasts().map((toast) => toast.message)).toEqual(['Saved']);

    vi.advanceTimersByTime(1);
    expect(toaster.toasts()).toEqual([]);
  });

  it('shows a message only once while it is on screen', () => {
    const toaster = TestBed.inject(Toaster);

    toaster.show('Server unreachable');
    toaster.show('Server unreachable');
    toaster.show('Saved');

    expect(toaster.toasts().map((toast) => toast.message)).toEqual([
      'Server unreachable',
      'Saved',
    ]);
  });
});
