import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import { Logo } from '../logo/logo';

@Component({
  selector: 'app-landing',
  imports: [RouterLink, TranslocoDirective, Logo],
  templateUrl: './landing.html',
  styleUrl: './landing.css',
})
export class Landing {}
