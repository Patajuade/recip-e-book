import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { form } from '@angular/forms/signals';
import {
  emptyItem,
  itemFormSchema,
  toItemFieldErrors,
  toItemFormValue,
  toItemInput,
} from './item-form';

describe('item form', () => {
  it('turns an item into form values and back', () => {
    const value = toItemFormValue({
      id: '1',
      name: 'farine',
      quantity: 500,
      unit: null,
      checked: true,
      position: 0,
      sourceRecipe: null,
    });

    expect(value).toEqual({ name: 'farine', quantity: 500, unit: '' });
    expect(toItemInput({ ...value, unit: ' g ' }, true)).toEqual({
      name: 'farine',
      quantity: 500,
      unit: 'g',
      checked: true,
    });
  });

  it('puts the API field errors of a 400 under their field', () => {
    const itemForm = TestBed.runInInjectionContext(() =>
      form(signal(emptyItem()), itemFormSchema),
    );
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: { Name: ['Required.'], Unit: ['Too long.'], Other: ['?'] },
      },
    });

    const errors = toItemFieldErrors(error, itemForm);

    expect(errors.map((e) => [e.message, e.fieldTree])).toEqual([
      ['Required.', itemForm.name],
      ['Too long.', itemForm.unit],
    ]);
  });

  it('has no field errors for any other failure', () => {
    const itemForm = TestBed.runInInjectionContext(() =>
      form(signal(emptyItem()), itemFormSchema),
    );

    expect(
      toItemFieldErrors(new HttpErrorResponse({ status: 500 }), itemForm),
    ).toEqual([]);
    expect(toItemFieldErrors(new Error('offline'), itemForm)).toEqual([]);
  });
});
