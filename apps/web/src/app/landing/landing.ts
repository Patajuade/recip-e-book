import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { Logo } from '../logo/logo';

@Component({
  selector: 'app-landing',
  imports: [RouterLink, TranslocoDirective, Logo],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {
  protected readonly session = inject(AccountSession);
}
