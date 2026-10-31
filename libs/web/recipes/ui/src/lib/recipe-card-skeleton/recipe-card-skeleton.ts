import { Component } from '@angular/core';

/** An empty recipe card shown while the recipes load. */
@Component({
  selector: 'lib-recipe-card-skeleton',
  templateUrl: './recipe-card-skeleton.html',
  styleUrl: './recipe-card-skeleton.css',
  host: { 'aria-hidden': 'true', class: 'block h-full' },
})
export class RecipeCardSkeleton {}
