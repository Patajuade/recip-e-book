import { Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { recipeListResource } from '@recip-e-book/web-recipes-data-access';
import { RecipeCard } from '@recip-e-book/web-recipes-ui';

@Component({
  selector: 'lib-recipe-list',
  imports: [TranslocoDirective, RecipeCard],
  templateUrl: './recipe-list.html',
  styleUrl: './recipe-list.css',
})
export class RecipeList {
  protected readonly recipes = recipeListResource();
}
