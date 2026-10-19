import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  ApplicationConfig,
  isDevMode,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import {
  provideRouter,
  TitleStrategy,
  withComponentInputBinding,
} from '@angular/router';
import { provideTransloco } from '@jsverse/transloco';
import { provideTranslocoPersistLang } from '@jsverse/transloco-persist-lang';
import {
  acceptLanguageInterceptor,
  LANGUAGES,
  pickInitialLang,
  provideDocumentLang,
  TranslocoTitleStrategy,
} from '@recip-e-book/web-shared-i18n';
import { appRoutes } from './app.routes';
import { TranslocoHttpLoader } from './transloco-loader';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(appRoutes, withComponentInputBinding()),
    { provide: TitleStrategy, useClass: TranslocoTitleStrategy },
    provideHttpClient(withInterceptors([acceptLanguageInterceptor])),
    provideTransloco({
      config: {
        availableLangs: LANGUAGES,
        defaultLang: 'en',
        // Remove this option if your application doesn't support changing language in runtime.
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
    provideTranslocoPersistLang({
      getLangFn: pickInitialLang,
      storage: { useValue: localStorage },
    }),
    provideDocumentLang(),
  ],
};
