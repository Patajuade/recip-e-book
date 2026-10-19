import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

/** Sends the active language (`en` or `fr`) to the API, so it can answer in the same language. */
export const acceptLanguageInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = inject(TranslocoService).getActiveLang();
  return next(req.clone({ headers: req.headers.set('Accept-Language', lang) }));
};
