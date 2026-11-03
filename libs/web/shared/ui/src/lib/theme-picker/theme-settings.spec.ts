import { DOCUMENT } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ThemeSettings } from './theme-settings';

describe('ThemeSettings', () => {
  afterEach(() => localStorage.clear());

  it('starts from the remembered theme and accent', () => {
    localStorage.setItem('recip-e-book.theme', 'nuit');
    localStorage.setItem('recip-e-book.accent', 'menthe');

    const settings = TestBed.inject(ThemeSettings);

    expect(settings.theme()).toBe('nuit');
    expect(settings.accent()).toBe('menthe');
  });

  it('falls back to the defaults for an unknown stored value', () => {
    localStorage.setItem('recip-e-book.theme', 'neon');

    const settings = TestBed.inject(ThemeSettings);

    expect(settings.theme()).toBe('cave');
    expect(settings.accent()).toBe('citron');
  });

  it('applies a choice to <html> and remembers it', () => {
    const settings = TestBed.inject(ThemeSettings);
    const root = TestBed.inject(DOCUMENT).documentElement;

    settings.theme.set('jardin');
    settings.accent.set('glacier');
    TestBed.tick();

    expect(root.dataset['theme']).toBe('jardin');
    expect(root.dataset['accent']).toBe('glacier');
    expect(localStorage.getItem('recip-e-book.theme')).toBe('jardin');
    expect(localStorage.getItem('recip-e-book.accent')).toBe('glacier');
  });
});
