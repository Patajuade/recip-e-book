import { HttpInterceptorFn } from '@angular/common/http';
import { inject, LOCALE_ID } from '@angular/core';

/**
 * Sends the locale of the current build (`en-US` or `fr`) to the API, so it
 * can answer in the same language. LOCALE_ID defaults to `$localize.locale`,
 * which each localized build sets.
 */
export const acceptLanguageInterceptor: HttpInterceptorFn = (req, next) => {
  const locale = inject(LOCALE_ID);
  return next(
    req.clone({ headers: req.headers.set('Accept-Language', locale) }),
  );
};
