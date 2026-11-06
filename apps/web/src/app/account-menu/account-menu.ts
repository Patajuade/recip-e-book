import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';

/** Who is signed in, or the way to sign in. Nothing while that is unknown (offline). */
@Component({
  selector: 'app-account-menu',
  imports: [TranslocoDirective, RouterLink],
  templateUrl: './account-menu.html',
  styleUrl: './account-menu.css',
  host: { class: 'flex min-w-0 items-center gap-2' },
})
export class AccountMenu {
  protected readonly session = inject(AccountSession);
  private readonly router = inject(Router);

  protected async signOut(): Promise<void> {
    try {
      await this.session.signOut();
    } catch {
      // Still signed in; the HTTP error interceptor already shows why.
      return;
    }
    await this.router.navigateByUrl('/');
  }
}
