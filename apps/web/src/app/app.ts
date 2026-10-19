import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LanguageSwitcher } from '@recip-e-book/web-shared-i18n';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LanguageSwitcher],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
