import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { TranslocoDecimalPipe } from '@jsverse/transloco-locale';
import { ShoppingListItem } from '@recip-e-book/web-shopping-models';

/** One item of a list: a big checkbox row, its source recipe, edit and remove buttons. */
@Component({
  selector: 'lib-shopping-item',
  imports: [TranslocoDirective, TranslocoDecimalPipe, RouterLink],
  templateUrl: './shopping-item.html',
  styleUrl: './shopping-item.css',
})
export class ShoppingItem {
  readonly item = input.required<ShoppingListItem>();

  readonly toggled = output();
  readonly editRequested = output();
  readonly removeRequested = output();
}
