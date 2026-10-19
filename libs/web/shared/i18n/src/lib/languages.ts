import { GetLangParams } from '@jsverse/transloco-persist-lang';

/** Languages of the app. Each label is written in its own language. */
export const LANGUAGES = [
  { id: 'en', label: 'English' },
  { id: 'fr', label: 'Français' },
];

/**
 * Initial language: the one the user picked last time, otherwise the browser
 * language when supported, otherwise the default one.
 */
export function pickInitialLang({
  cachedLang,
  browserLang,
  defaultLang,
}: GetLangParams): string {
  const supported = LANGUAGES.map((language) => language.id);
  if (cachedLang && supported.includes(cachedLang)) {
    return cachedLang;
  }
  if (browserLang && supported.includes(browserLang)) {
    return browserLang;
  }
  return defaultLang;
}
