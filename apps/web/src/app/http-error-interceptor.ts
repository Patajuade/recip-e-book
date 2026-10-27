import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { Toaster } from '@recip-e-book/web-shared-ui';
import { NetworkStatus } from '@recip-e-book/web-shared-util';
import { tap } from 'rxjs';

// 0: network error or timeout. 502–504: a proxy couldn't reach the API (the dev server proxy
// answers 502 when the API is down).
const UNREACHABLE = new Set([0, 502, 503, 504]);

/**
 * Tells why an API call failed when the request itself isn't at fault: API unreachable or
 * server error (5xx). Screens still show what failed; 4xx are theirs to handle (validation,
 * not found). The error is passed on unchanged.
 */
export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const network = inject(NetworkStatus);
  const toaster = inject(Toaster);
  const transloco = inject(TranslocoService);
  return next(req).pipe(
    tap({
      error: (error: unknown) => {
        if (
          !req.url.startsWith('/api/') ||
          !(error instanceof HttpErrorResponse)
        ) {
          return;
        }
        if (UNREACHABLE.has(error.status) && !network.online()) {
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
