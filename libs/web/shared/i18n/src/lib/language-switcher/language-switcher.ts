import { Component, inject, LOCALE_ID } from '@angular/core';

@Component({
  selector: 'lib-language-switcher',
  imports: [],
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.css',
})
export class LanguageSwitcher {
  protected readonly currentLocale = inject(LOCALE_ID);
  protected readonly locales = [
    { id: 'en-US', label: 'English', href: '/en-US/' },
    { id: 'fr', label: 'Français', href: '/fr/' },
  ];
}
