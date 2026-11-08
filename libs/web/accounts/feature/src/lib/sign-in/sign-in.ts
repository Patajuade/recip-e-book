import { Component, inject, input, signal } from '@angular/core';
import { form, FormField, FormRoot } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import {
  AccountSession,
  safeReturnUrl,
} from '@recip-e-book/web-accounts-data-access';
import { LabeledField } from '@recip-e-book/web-shared-ui';
import {
  Credentials,
  emptyCredentials,
  isStatus,
  signInSchema,
} from '../account-form';

@Component({
  selector: 'lib-sign-in',
  imports: [TranslocoDirective, RouterLink, FormField, FormRoot, LabeledField],
  templateUrl: './sign-in.html',
  styleUrl: './sign-in.css',
})
export class SignIn {
  /** Where to go once signed in (query parameter, set by the guard). */
  readonly returnUrl = input<string>();

  protected readonly session = inject(AccountSession);
  private readonly router = inject(Router);

  // Not submission errors: those block submit() until a value changes, so a plain retry
  // would do nothing.
  protected readonly failure = signal<'refused' | 'unavailable' | null>(null);

  private readonly credentials = signal<Credentials>(emptyCredentials());
  protected readonly signInForm = form(this.credentials, signInSchema, {
    submission: {
      action: async () => {
        this.failure.set(null);
        try {
          await this.session.signIn(this.credentials());
        } catch (error) {
          this.failure.set(isStatus(error, 401) ? 'refused' : 'unavailable');
          return undefined;
        }
        await this.router.navigateByUrl(safeReturnUrl(this.returnUrl()));
        return undefined;
      },
    },
  });
}
