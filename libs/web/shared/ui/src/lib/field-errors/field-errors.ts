import { Component, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'lib-field-errors',
  imports: [TranslocoDirective],
  templateUrl: './field-errors.html',
  styleUrl: './field-errors.css',
})
export class FieldErrors {
  readonly field = input.required<FieldTree<unknown>>();
}
