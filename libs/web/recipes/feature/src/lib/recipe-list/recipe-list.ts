import { TranslocoDirective } from '@jsverse/transloco';
import { recipeListResource } from '@recip-e-book/web-recipes-data-access';
import { RecipeCard, RecipeCardSkeleton } from '@recip-e-book/web-recipes-ui';
import { LoadMore } from '@recip-e-book/web-shared-ui';
import { delayedLoading } from '@recip-e-book/web-shared-util';
import { Component, computed, inject, input } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'lib-recipe-list',
  imports: [
    TranslocoDirective,
    RecipeCard,
    RecipeCardSkeleton,
    RouterLink,
    LoadMore,
  ],
  templateUrl: './recipe-list.html',
  styleUrl: './recipe-list.css',
})
export class RecipeList {
  readonly search = input<string>();
  readonly tag = input<string>();

  protected readonly recipes = recipeListResource(() => ({
    search: this.search(),
    tag: this.tag(),
  }));

  // A reload after a write keeps the cards on screen: only a load without a value waits.
  protected readonly waiting = computed(
    () => this.recipes.isLoading() && !this.recipes.hasValue(),
  );
  protected readonly skeleton = delayedLoading(this.waiting);

  private readonly router = inject(Router);
  private readonly searchTerms = new Subject<string>();

  constructor() {
    this.searchTerms
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((search) =>
        this.router.navigate(['/recipes'], {
          queryParams: { search: search.trim() || null },
          queryParamsHandling: 'merge',
          replaceUrl: true,
        }),
      );
  }

  protected onSearch(value: string): void {
    this.searchTerms.next(value);
  }
}
