import { Routes } from '@angular/router';
import { ShoppingLists } from './shopping-lists/shopping-lists';

export const shoppingRoutes: Routes = [
  {
    path: '',
    component: ShoppingLists,
    title: 'shopping.lists.pageTitle',
  },
];
