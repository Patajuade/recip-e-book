import {
  afterRenderEffect,
  booleanAttribute,
  Component,
  contentChild,
  ElementRef,
  input,
} from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldErrors } from '../field-errors/field-errors';

let nextId = 0;

/**
 * A form field: its label, the projected control (`[formField]`) and its errors.
 * `[formField]` sets no ARIA attributes, so the control is linked to its errors here.
 */
@Component({
  selector: 'lib-labeled-field',
  imports: [FieldErrors],
  templateUrl: './labeled-field.html',
  styleUrl: './labeled-field.css',
  host: { class: 'flex flex-col' },
})
export class LabeledField {
  readonly label = input.required<string>();
  readonly field = input.required<FieldTree<unknown>>();
  /** Keeps the label for screen readers only (e.g. inputs laid out in a row). */
  readonly labelHidden = input(false, { transform: booleanAttribute });

  private readonly id = nextId++;
  protected readonly controlId = `lib-labeled-field-${this.id}`;
  protected readonly errorsId = `lib-labeled-field-errors-${this.id}`;
  private readonly control = contentChild(FormField, { read: ElementRef });

  constructor() {
    afterRenderEffect({
      write: () => {
        const element = this.control()?.nativeElement as
          HTMLElement | undefined;
        const state = this.field()();
        element?.setAttribute('id', this.controlId);
        element?.setAttribute(
          'aria-invalid',
          String(state.touched() && state.invalid()),
        );
        element?.setAttribute('aria-describedby', this.errorsId);
      },
    });
  }
}
