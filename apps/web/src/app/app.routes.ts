import { Route } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';
import { NotFound } from './not-found/not-found';

export const appRoutes: Route[] = [
  { path: '', redirectTo: '/recipes', pathMatch: 'full' },
  {
    path: 'recipes',
    loadChildren: () =>
      import('@recip-e-book/web-recipes-feature').then((m) => m.recipesRoutes),
  },
  {
    path: 'shopping',
    loadChildren: () =>
      import('@recip-e-book/web-shopping-feature').then(
        (m) => m.shoppingRoutes,
      ),
  },
  {
    path: '**',
    component: NotFound,
    title: marker('notFound.pageTitle'),
  },
];
