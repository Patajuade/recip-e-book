import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountSession } from './account-session';

/** For the pages meant for visitors: someone signed in goes straight to their recipes. */
export const visitorOnlyGuard: CanActivateFn = async () => {
  const session = inject(AccountSession);
  const router = inject(Router);
  await session.ready;
  return session.signedIn() ? router.parseUrl('/recipes') : true;
};
