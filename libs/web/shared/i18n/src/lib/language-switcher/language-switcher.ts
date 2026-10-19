import { Component, inject } from '@angular/core';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import { LANGUAGES } from '../languages';

@Component({
  selector: 'lib-language-switcher',
  imports: [TranslocoDirective],
  templateUrl: './language-switcher.html',
  styleUrl: './language-switcher.css',
})
export class LanguageSwitcher {
  private readonly transloco = inject(TranslocoService);
  protected readonly languages = LANGUAGES;
  protected readonly activeLang = this.transloco.activeLang;

  protected select(lang: string): void {
    this.transloco.setActiveLang(lang);
  }
}
