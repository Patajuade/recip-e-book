import { DOCUMENT, effect, inject, Service, signal } from '@angular/core';

export const THEMES = ['cave', 'sousbois', 'nuit'] as const;
export const ACCENTS = ['citron', 'safran', 'menthe', 'glacier'] as const;
export type Theme = (typeof THEMES)[number];
export type Accent = (typeof ACCENTS)[number];

// Also read by the inline script in apps/web/src/index.html, before the first paint.
const THEME_KEY = 'recip-e-book.theme';
const ACCENT_KEY = 'recip-e-book.accent';

/** The chosen background and accent, applied to <html> (styles.css) and remembered. */
@Service()
export class ThemeSettings {
  readonly theme = signal<Theme>(stored(THEME_KEY, THEMES, 'cave'));
  readonly accent = signal<Accent>(stored(ACCENT_KEY, ACCENTS, 'citron'));

  private readonly root = inject(DOCUMENT).documentElement;

  constructor() {
    effect(() => this.apply('theme', THEME_KEY, this.theme()));
    effect(() => this.apply('accent', ACCENT_KEY, this.accent()));
  }

  private apply(
    attribute: 'theme' | 'accent',
    key: string,
    value: string,
  ): void {
    this.root.dataset[attribute] = value;
    try {
      localStorage.setItem(key, value);
    } catch {
      // Storage can be unavailable (private mode, blocked site data): the choice lasts the visit.
    }
  }
}

function stored<T extends string>(
  key: string,
  allowed: readonly T[],
  fallback: T,
): T {
  try {
    const value = localStorage.getItem(key);
    return allowed.find((option) => option === value) ?? fallback;
  } catch {
    return fallback;
  }
}
