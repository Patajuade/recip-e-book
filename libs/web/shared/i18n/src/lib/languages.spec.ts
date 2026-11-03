import { pickInitialLang } from './languages';

describe('pickInitialLang', () => {
  it('prefers the language picked last time', () => {
    expect(
      pickInitialLang({
        cachedLang: 'fr',
        browserLang: 'en',
        cultureLang: 'en-US',
        defaultLang: 'en',
      }),
    ).toBe('fr');
  });

  it('then the browser language when the app has it', () => {
    expect(
      pickInitialLang({
        cachedLang: null,
        browserLang: 'fr',
        cultureLang: 'fr-BE',
        defaultLang: 'en',
      }),
    ).toBe('fr');
  });

  it('then the default language', () => {
    expect(
      pickInitialLang({
        cachedLang: 'de',
        browserLang: 'es',
        cultureLang: 'es-ES',
        defaultLang: 'en',
      }),
    ).toBe('en');
  });
});
