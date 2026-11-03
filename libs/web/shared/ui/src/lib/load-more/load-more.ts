import {
  afterNextRender,
  Component,
  DOCUMENT,
  effect,
  Injector,
  inject,
  input,
  output,
} from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { NearViewport } from './near-viewport';

/**
 * « Charger plus » below a paged list. Loads by itself when it nears the viewport; the button
 * stays for the keyboard and screen readers. After a click, focus moves to the first new item
 * of `list`, so keyboard users carry on from there.
 */
@Component({
  selector: 'lib-load-more',
  imports: [TranslocoDirective, NearViewport],
  templateUrl: './load-more.html',
  styleUrl: './load-more.css',
})
export class LoadMore {
  readonly hasMore = input.required<boolean>();
  readonly loading = input.required<boolean>();
  readonly failed = input.required<boolean>();
  /** The element whose children are the list items. */
  readonly list = input.required<HTMLElement>();
  readonly loadMore = output<void>();

  private readonly injector = inject(Injector);
  private readonly document = inject(DOCUMENT);
  private focusFrom: number | null = null;

  constructor() {
    effect(() => {
      if (this.loading() || this.focusFrom === null) {
        return;
      }
      const from = this.focusFrom;
      this.focusFrom = null;
      if (this.failed()) {
        return;
      }
      afterNextRender(
        () =>
          this.list()
            .children[from]?.querySelector<HTMLElement>('a, button')
            ?.focus(),
        { injector: this.injector },
      );
    });
  }

  protected loadByClick(): void {
    this.focusFrom = this.list().children.length;
    this.loadMore.emit();
  }

  // Tabbing to the button scrolls it into view, which loads the next page: the new items then
  // come before the focused button, so focus moves to them as after a click.
  protected loadByScroll(button: HTMLElement): void {
    if (this.document.activeElement === button) {
      this.loadByClick();
    } else {
      this.loadMore.emit();
    }
  }
}
