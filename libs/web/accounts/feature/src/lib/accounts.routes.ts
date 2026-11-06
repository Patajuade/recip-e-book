import { Routes } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';
import { visitorOnlyGuard } from '@recip-e-book/web-accounts-data-access';

export const accountsRoutes: Routes = [
  {
    path: 'sign-in',
    loadComponent: () => import('./sign-in/sign-in').then((m) => m.SignIn),
    title: marker('accounts.signIn.pageTitle'),
    canActivate: [visitorOnlyGuard],
  },
  {
    path: 'sign-up',
    loadComponent: () => import('./sign-up/sign-up').then((m) => m.SignUp),
    title: marker('accounts.signUp.pageTitle'),
    canActivate: [visitorOnlyGuard],
  },
];
