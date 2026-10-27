import { TranslocoGlobalConfig } from '@jsverse/transloco-utils';

const config: TranslocoGlobalConfig = {
  rootTranslationsPath: 'apps/web/public/i18n/',
  langs: ['en', 'fr'],
  keysManager: {
    // The app and every web lib use keys from the app's translation files.
    input: ['apps/web/src', 'libs/web'],
  },
};

export default config;
