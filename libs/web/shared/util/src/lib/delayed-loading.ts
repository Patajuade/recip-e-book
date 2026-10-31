import { effect, Signal, signal, untracked } from '@angular/core';

/**
 * Whether to show a loading skeleton while `waiting()` is true, without flicker: nothing for
 * the first `delay` ms, so a fast answer shows no skeleton at all, then at least `minDuration`
 * ms once shown, so it can't appear and vanish in a flash. Needs an injection context.
 */
export function delayedLoading(
  waiting: () => boolean,
  { delay = 300, minDuration = 500 } = {},
): Signal<boolean> {
  const shown = signal(false);
  let shownAt = 0;

  effect((onCleanup) => {
    const isWaiting = waiting();
    if (isWaiting === untracked(shown)) {
      return;
    }
    const timer = isWaiting
      ? setTimeout(() => {
          shownAt = Date.now();
          shown.set(true);
        }, delay)
      : setTimeout(
          () => shown.set(false),
          Math.max(0, shownAt + minDuration - Date.now()),
        );
    onCleanup(() => clearTimeout(timer));
  });

  return shown.asReadonly();
}
