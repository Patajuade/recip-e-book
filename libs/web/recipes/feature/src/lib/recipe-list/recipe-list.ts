import { Component } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'lib-recipe-list',
  imports: [TranslocoDirective],
  templateUrl: './recipe-list.html',
  styleUrl: './recipe-list.css',
})
export class RecipeList {}
