import { inject, Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { Subscription } from 'rxjs';

/**
 * Route `title`s are translation keys. The tab title is translated, and
 * translated again when the active language changes.
 */
@Injectable({ providedIn: 'root' })
export class TranslocoTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);
  private readonly transloco = inject(TranslocoService);
  private subscription?: Subscription;

  override updateTitle(snapshot: RouterStateSnapshot): void {
    const key = this.buildTitle(snapshot);
    this.subscription?.unsubscribe();
    if (key) {
      this.subscription = this.transloco
        .selectTranslate(key)
        .subscribe((title) => this.title.setTitle(title));
    }
  }
}
