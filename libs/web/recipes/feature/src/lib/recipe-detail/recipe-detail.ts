import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import {
  TranslocoDatePipe,
  TranslocoDecimalPipe,
} from '@jsverse/transloco-locale';
import { recipeDetailResource } from '@recip-e-book/web-recipes-data-access';
import { DurationPipe } from '@recip-e-book/web-shared-util';

@Component({
  selector: 'lib-recipe-detail',
  imports: [
    RouterLink,
    TranslocoDirective,
    TranslocoDatePipe,
    TranslocoDecimalPipe,
    DurationPipe,
  ],
  templateUrl: './recipe-detail.html',
  styleUrl: './recipe-detail.css',
})
export class RecipeDetail {
  readonly id = input.required<string>();

  protected readonly recipe = recipeDetailResource(this.id);
  protected readonly lang = inject(TranslocoService).activeLang;

  protected readonly notFound = computed(() => {
    const error = this.recipe.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });
}
