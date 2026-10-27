import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { LanguageSwitcher } from '@recip-e-book/web-shared-i18n';
import { ThemePicker, ToastList } from '@recip-e-book/web-shared-ui';
import { NetworkStatus } from '@recip-e-book/web-shared-util';
import { AppUpdates } from './app-updates';

@Component({
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    TranslocoDirective,
    LanguageSwitcher,
    ThemePicker,
    ToastList,
  ],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly network = inject(NetworkStatus);
  protected readonly updates = inject(AppUpdates);
}
