import { Route } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';
import {
  signedInGuard,
  visitorOnlyGuard,
} from '@recip-e-book/web-accounts-data-access';
import { NotFound } from './not-found/not-found';

export const appRoutes: Route[] = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./landing/landing').then((m) => m.Landing),
    title: marker('landing.pageTitle'),
    canActivate: [visitorOnlyGuard],
  },
  {
    path: 'account',
    loadChildren: () =>
      import('@recip-e-book/web-accounts-feature').then(
        (m) => m.accountsRoutes,
      ),
  },
  {
    path: 'recipes',
    canActivate: [signedInGuard],
    loadChildren: () =>
      import('@recip-e-book/web-recipes-feature').then((m) => m.recipesRoutes),
  },
  {
    path: 'shopping',
    canActivate: [signedInGuard],
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
