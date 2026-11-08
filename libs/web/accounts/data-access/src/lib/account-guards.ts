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

/**
 * For the pages with personal data: a visitor goes to sign in, then comes back here. When the
 * session is unknown (offline), the page opens: what was already read stays readable, and the
 * API decides anyway.
 */
export const signedInGuard: CanActivateFn = async (_route, state) => {
  const session = inject(AccountSession);
  const router = inject(Router);
  await session.ready;
  return session.signedIn() || !session.known()
    ? true
    : router.createUrlTree(['/account/sign-in'], {
        queryParams: { returnUrl: state.url },
      });
};

/** Only a path of this app: never another site (`//example.com`), whatever the URL says. */
export const safeReturnUrl = (url: string | undefined): string =>
  url && url.startsWith('/') && !url.startsWith('//') && !url.startsWith('/\\')
    ? url
    : '/recipes';
