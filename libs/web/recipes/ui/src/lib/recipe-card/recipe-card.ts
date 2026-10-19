import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { RecipeSummary } from '@recip-e-book/web-recipes-models';

@Component({
  selector: 'lib-recipe-card',
  imports: [RouterLink, TranslocoDirective],
  templateUrl: './recipe-card.html',
  styleUrl: './recipe-card.css',
})
export class RecipeCard {
  readonly recipe = input.required<RecipeSummary>();
}
