import { Component, input } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'lib-recipe-edit',
  imports: [TranslocoDirective],
  templateUrl: './recipe-edit.html',
  styleUrl: './recipe-edit.css',
})
export class RecipeEdit {
  readonly id = input<string>();
}
