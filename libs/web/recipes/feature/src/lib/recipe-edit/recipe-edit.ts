import { Component, input } from '@angular/core';

@Component({
  selector: 'lib-recipe-edit',
  imports: [],
  templateUrl: './recipe-edit.html',
  styleUrl: './recipe-edit.css',
})
export class RecipeEdit {
  readonly id = input<string>();
}
