import { Component } from '@angular/core';

/** An empty shopping list item shown while a list loads. */
@Component({
  selector: 'lib-shopping-item-skeleton',
  templateUrl: './shopping-item-skeleton.html',
  styleUrl: './shopping-item-skeleton.css',
  host: { 'aria-hidden': 'true', class: 'block' },
})
export class ShoppingItemSkeleton {}
