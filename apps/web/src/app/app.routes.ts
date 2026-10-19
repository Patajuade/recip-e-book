import { Route } from '@angular/router';
import { NotFound } from './not-found/not-found';

export const appRoutes: Route[] = [
  { path: '', redirectTo: '/recipes', pathMatch: 'full' },
  {
    path: 'recipes',
    loadChildren: () =>
      import('@recip-e-book/web-recipes-feature').then((m) => m.recipesRoutes),
  },
  {
    path: '**',
    component: NotFound,
    title: 'notFound.pageTitle',
  },
];
