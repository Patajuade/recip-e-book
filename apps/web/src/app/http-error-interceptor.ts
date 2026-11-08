import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { Toaster } from '@recip-e-book/web-shared-ui';
import { NetworkStatus } from '@recip-e-book/web-shared-util';
import { tap } from 'rxjs';

// 0: network error or timeout. 502–504: a proxy couldn't reach the API (the dev server proxy
// answers 502 when the API is down).
const UNREACHABLE = new Set([0, 502, 503, 504]);

/**
 * Tells why an API call failed when the request itself isn't at fault: API unreachable or
 * server error (5xx). Screens still show what failed; 4xx are theirs to handle (validation,
 * not found). The error is passed on unchanged. The session is read at every start: without
 * network the app stays usable, so that failure says nothing to the user. A 401 means the
 * session ended (expired, or signed out in another tab): back to signing in, then here.
 */
export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const network = inject(NetworkStatus);
  const toaster = inject(Toaster);
  const transloco = inject(TranslocoService);
  // Resolved only when needed: AccountSession's own first request goes through here.
  const injector = inject(Injector);
  const router = inject(Router);
  return next(req).pipe(
    tap({
      error: (error: unknown) => {
        if (
          !req.url.startsWith('/api/') ||
          req.url === '/api/auth/session' ||
          !(error instanceof HttpErrorResponse)
        ) {
          return;
        }
        if (error.status === 401 && !req.url.startsWith('/api/auth/')) {
          const returnUrl = router.url;
          void injector
            .get(AccountSession)
            .refresh()
            .then(() =>
              router.navigate(['/account/sign-in'], {
                queryParams: { returnUrl },
              }),
            );
        } else if (UNREACHABLE.has(error.status) && !network.online()) {
          // Offline, the service worker answers what it has cached; this one wasn't.
          toaster.show(
            req.method === 'GET'
              ? transloco.translate('httpErrors.offlineRead')
              : transloco.translate('httpErrors.offlineWrite'),
          );
        } else if (UNREACHABLE.has(error.status)) {
          toaster.show(transloco.translate('httpErrors.unreachable'));
        } else if (error.status >= 500) {
          toaster.show(transloco.translate('httpErrors.server'));
        }
      },
    }),
  );
};
