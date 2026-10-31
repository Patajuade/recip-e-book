import {
  DestroyRef,
  Directive,
  effect,
  ElementRef,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';

/**
 * Emits when the element comes within 300 px of the viewport. Paused with
 * `nearViewportEnabled`, it emits again on resume if the element is still near, so a list
 * keeps loading until it fills the screen.
 */
@Directive({
  selector: '[libNearViewport]',
})
export class NearViewport {
  readonly libNearViewport = output<void>();
  readonly nearViewportEnabled = input(true);

  private readonly near = signal(false);

  constructor() {
    if (typeof IntersectionObserver === 'undefined') {
      return;
    }
    const element = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const observer = new IntersectionObserver(
      ([entry]) => this.near.set(entry.isIntersecting),
      { rootMargin: '300px' },
    );
    inject(DestroyRef).onDestroy(() => observer.disconnect());

    // Observing again on resume: the last known state predates what was just added (the
    // element may now be far below), and observe() reports where the element is now.
    effect(() => {
      if (!this.nearViewportEnabled()) {
        untracked(() => this.near.set(false));
        return;
      }
      observer.unobserve(element);
      observer.observe(element);
    });

    effect(() => {
      if (this.near() && this.nearViewportEnabled()) {
        untracked(() => this.libNearViewport.emit());
      }
    });
  }
}
