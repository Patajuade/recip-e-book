import { Component, inject, signal } from '@angular/core';
import { form, FormField, FormRoot } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { LabeledField } from '@recip-e-book/web-shared-ui';
import {
  Credentials,
  emptyCredentials,
  isStatus,
  signUpSchema,
  toCredentialErrors,
} from '../account-form';

@Component({
  selector: 'lib-sign-up',
  imports: [TranslocoDirective, RouterLink, FormField, FormRoot, LabeledField],
  templateUrl: './sign-up.html',
  styleUrl: './sign-up.css',
})
export class SignUp {
  protected readonly session = inject(AccountSession);
  private readonly router = inject(Router);

  // A failure that isn't about a field. Not a submission error: those block submit() until a
  // value changes, so a plain retry would do nothing.
  protected readonly failed = signal(false);

  private readonly credentials = signal<Credentials>(emptyCredentials());
  protected readonly signUpForm = form(this.credentials, signUpSchema, {
    submission: {
      action: async (signUpForm) => {
        this.failed.set(false);
        try {
          await this.session.signUp(this.credentials());
        } catch (error) {
          if (isStatus(error, 403)) {
            // Closed since this page opened: the page now says so.
            await this.session.refresh();
            return undefined;
          }
          const fieldErrors = toCredentialErrors(error, signUpForm);
          this.failed.set(fieldErrors.length === 0);
          return fieldErrors;
        }
        await this.router.navigateByUrl('/recipes');
        return undefined;
      },
    },
  });
}
