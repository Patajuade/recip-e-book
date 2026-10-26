import { httpResource } from '@angular/common/http';
import {
  ShoppingListDetails,
  ShoppingListSummary,
} from '@recip-e-book/web-shopping-models';

export function shoppingListsResource() {
  return httpResource<ShoppingListSummary[]>(() => '/api/shopping-lists');
}

export function shoppingListResource(id: () => string | undefined) {
  return httpResource<ShoppingListDetails>(() => {
    const listId = id();
    return listId
      ? `/api/shopping-lists/${encodeURIComponent(listId)}`
      : undefined;
  });
}
