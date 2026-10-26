import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { TranslocoDecimalPipe } from '@jsverse/transloco-locale';
import { recipeDetailResource } from '@recip-e-book/web-recipes-data-access';
import { ScreenWakeLock } from '@recip-e-book/web-shared-util';
import { CookingTimers } from './cooking-timers';
import { splitDurations } from '../step-durations';

const SWIPE_DISTANCE_PX = 60;

/** Cooking mode: one step at a time, in big text, for a recipe on the counter. */
@Component({
  selector: 'lib-recipe-cook',
  imports: [TranslocoDirective, TranslocoDecimalPipe, RouterLink],
  templateUrl: './recipe-cook.html',
  styleUrl: './recipe-cook.css',
  providers: [ScreenWakeLock, CookingTimers],
  host: {
    '(document:keydown.arrowright)': 'next()',
    '(document:keydown.arrowleft)': 'previous()',
  },
})
export class RecipeCook {
  readonly id = input.required<string>();

  protected readonly recipe = recipeDetailResource(this.id);

  protected readonly notFound = computed(() => {
    const error = this.recipe.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  protected readonly steps = computed(() =>
    this.recipe.hasValue() ? this.recipe.value().steps : [],
  );
  protected readonly index = signal(0);
  protected readonly segments = computed(() =>
    splitDurations(this.steps()[this.index()] ?? ''),
  );
  /** Indexes of the ingredients checked while cooking (not saved). */
  protected readonly checked = signal<ReadonlySet<number>>(new Set());
  protected readonly isFirst = computed(() => this.index() === 0);
  protected readonly isLast = computed(
    () => this.index() === this.steps().length - 1,
  );

  protected readonly wakeLock = inject(ScreenWakeLock);
  protected readonly timers = inject(CookingTimers);
  private readonly router = inject(Router);
  private swipeStartX: number | null = null;

  protected next(): void {
    this.index.update((i) => Math.min(i + 1, this.steps().length - 1));
  }

  protected previous(): void {
    this.index.update((i) => Math.max(i - 1, 0));
  }

  protected toggleIngredient(index: number): void {
    this.checked.update((checked) => {
      const next = new Set(checked);
      if (!next.delete(index)) {
        next.add(index);
      }
      return next;
    });
  }

  /** m:ss, or h:mm:ss from one hour. */
  protected format(ms: number): string {
    const total = Math.ceil(ms / 1000);
    const hours = Math.floor(total / 3600);
    const minutes = Math.floor((total % 3600) / 60);
    const seconds = String(total % 60).padStart(2, '0');
    return hours
      ? `${hours}:${String(minutes).padStart(2, '0')}:${seconds}`
      : `${minutes}:${seconds}`;
  }

  protected finish(): void {
    void this.router.navigate(['/recipes', this.id()]);
  }

  protected swipeStart(event: PointerEvent): void {
    this.swipeStartX = event.clientX;
  }

  protected swipeEnd(event: PointerEvent): void {
    if (this.swipeStartX === null) {
      return;
    }
    const distance = event.clientX - this.swipeStartX;
    this.swipeStartX = null;
    if (distance <= -SWIPE_DISTANCE_PX) {
      this.next();
    } else if (distance >= SWIPE_DISTANCE_PX) {
      this.previous();
    }
  }
}
