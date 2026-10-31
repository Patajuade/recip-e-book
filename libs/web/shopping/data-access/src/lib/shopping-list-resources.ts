import { httpResource } from '@angular/common/http';
import {
  ShoppingListDetails,
  ShoppingListSummary,
} from '@recip-e-book/web-shopping-models';
import { pagedResource } from '@recip-e-book/web-shared-util';

/** The lists, a page at a time (`limit`: page size, the API's default when omitted). */
export function shoppingListsResource(limit?: number) {
  return pagedResource<ShoppingListSummary>(() => ({
    url: '/api/shopping-lists',
    params: limit ? { limit } : undefined,
  }));
}

export function shoppingListResource(id: () => string | undefined) {
  return httpResource<ShoppingListDetails>(() => {
    const listId = id();
    return listId
      ? `/api/shopping-lists/${encodeURIComponent(listId)}`
      : undefined;
  });
}
