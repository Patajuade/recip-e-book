import { Routes } from '@angular/router';
import { ShoppingList } from './shopping-list/shopping-list';
import { ShoppingLists } from './shopping-lists/shopping-lists';

export const shoppingRoutes: Routes = [
  {
    path: '',
    component: ShoppingLists,
    title: 'shopping.lists.pageTitle',
  },
  {
    path: ':id',
    component: ShoppingList,
    title: 'shopping.list.pageTitle',
  },
];
