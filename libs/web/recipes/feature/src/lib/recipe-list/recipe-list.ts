import { TranslocoDirective } from '@jsverse/transloco';
import { recipeListResource } from '@recip-e-book/web-recipes-data-access';
import { RecipeCard } from '@recip-e-book/web-recipes-ui';
import { Component, inject, input } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

@Component({
  selector: 'lib-recipe-list',
  imports: [TranslocoDirective, RecipeCard, RouterLink],
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
