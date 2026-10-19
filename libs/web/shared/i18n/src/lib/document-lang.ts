import {
  DOCUMENT,
  effect,
  EnvironmentProviders,
  inject,
  provideEnvironmentInitializer,
} from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

/** Keeps `<html lang>` in sync with the active language (screen readers, spell checking). */
export function provideDocumentLang(): EnvironmentProviders {
  return provideEnvironmentInitializer(() => {
    const document = inject(DOCUMENT);
    const transloco = inject(TranslocoService);
    effect(() => {
      document.documentElement.lang = transloco.activeLang();
    });
  });
}
