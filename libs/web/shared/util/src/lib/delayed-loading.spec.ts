import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { delayedLoading } from './delayed-loading';

describe('delayedLoading', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  function setup() {
    const waiting = signal(false);
    const shown = TestBed.runInInjectionContext(() => delayedLoading(waiting));
    // Effects run on TestBed.tick(); the timers they start run on advanceTimersByTime().
    const wait = (ms: number) => {
      TestBed.tick();
      vi.advanceTimersByTime(ms);
      TestBed.tick();
    };
    return { waiting, shown, wait };
  }

  it('shows nothing when the data arrives within the delay', () => {
    const { waiting, shown, wait } = setup();

    waiting.set(true);
    wait(299);
    waiting.set(false);
    wait(1000);

    expect(shown()).toBe(false);
  });

  it('shows the skeleton once the delay has passed', () => {
    const { waiting, shown, wait } = setup();

    waiting.set(true);
    wait(300);

    expect(shown()).toBe(true);
  });

  it('keeps it shown for the minimum duration', () => {
    const { waiting, shown, wait } = setup();

    waiting.set(true);
    wait(300);
    waiting.set(false);
    wait(499);
    expect(shown()).toBe(true);

    wait(1);
    expect(shown()).toBe(false);
  });

  it('hides it at once after a long wait', () => {
    const { waiting, shown, wait } = setup();

    waiting.set(true);
    wait(2000);
    waiting.set(false);
    wait(0);

    expect(shown()).toBe(false);
  });
});
