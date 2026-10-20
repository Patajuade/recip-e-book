import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { RecipeSummary } from '@recip-e-book/web-recipes-models';
import { DurationPipe } from '@recip-e-book/web-shared-util';

@Component({
  selector: 'lib-recipe-card',
  imports: [RouterLink, DurationPipe],
  templateUrl: './recipe-card.html',
  styleUrl: './recipe-card.css',
})
export class RecipeCard {
  readonly recipe = input.required<RecipeSummary>();

  protected readonly lang = inject(TranslocoService).activeLang;
}
