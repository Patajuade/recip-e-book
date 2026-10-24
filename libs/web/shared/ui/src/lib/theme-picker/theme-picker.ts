import {
  CdkMenu,
  CdkMenuGroup,
  CdkMenuItemRadio,
  CdkMenuTrigger,
} from '@angular/cdk/menu';
import { Component, inject } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';
import { ACCENTS, ThemeSettings, THEMES } from './theme-settings';

/** Header menu to pick the background and the accent, each shown as a swatch. */
@Component({
  selector: 'lib-theme-picker',
  imports: [
    TranslocoDirective,
    CdkMenuTrigger,
    CdkMenu,
    CdkMenuGroup,
    CdkMenuItemRadio,
  ],
  templateUrl: './theme-picker.html',
  styleUrl: './theme-picker.css',
})
export class ThemePicker {
  protected readonly settings = inject(ThemeSettings);
  protected readonly themes = THEMES;
  protected readonly accents = ACCENTS;
}
