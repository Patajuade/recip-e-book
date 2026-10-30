import { Routes } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';

export const shoppingRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./shopping-lists/shopping-lists').then((m) => m.ShoppingLists),
    title: marker('shopping.lists.pageTitle'),
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./shopping-list/shopping-list').then((m) => m.ShoppingList),
    title: marker('shopping.list.pageTitle'),
  },
];
