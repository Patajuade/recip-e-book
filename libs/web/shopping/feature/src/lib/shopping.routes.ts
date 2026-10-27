import { Routes } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';
import { ShoppingList } from './shopping-list/shopping-list';
import { ShoppingLists } from './shopping-lists/shopping-lists';

export const shoppingRoutes: Routes = [
  {
    path: '',
    component: ShoppingLists,
    title: marker('shopping.lists.pageTitle'),
  },
  {
    path: ':id',
    component: ShoppingList,
    title: marker('shopping.list.pageTitle'),
  },
];
