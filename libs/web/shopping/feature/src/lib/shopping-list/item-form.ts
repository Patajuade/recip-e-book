import { HttpErrorResponse } from '@angular/common/http';
import {
  FieldTree,
  max,
  maxLength,
  min,
  required,
  schema,
  ValidationError,
  WithOptionalFieldTree,
} from '@angular/forms/signals';
import {
  ShoppingListItem,
  ShoppingListItemInput,
} from '@recip-e-book/web-shopping-models';

/** Form model of an item: empty texts are '', an empty quantity is null. */
export interface ItemFormValue {
  name: string;
  quantity: number | null;
  unit: string;
}

export const emptyItem = (): ItemFormValue => ({
  name: '',
  quantity: null,
  unit: '',
});

export const toItemFormValue = (item: ShoppingListItem): ItemFormValue => ({
  name: item.name,
  quantity: item.quantity,
  unit: item.unit ?? '',
});

// Mirrors the API rules (ShoppingListItemInput).
export const itemFormSchema = schema<ItemFormValue>((item) => {
  required(item.name);
  maxLength(item.name, 200);
  min(item.quantity, 0);
  max(item.quantity, 100_000);
  maxLength(item.unit, 50);
});

export const toItemInput = (
  item: ItemFormValue,
  checked: boolean,
): ShoppingListItemInput => ({
  name: item.name,
  quantity: item.quantity,
  unit: item.unit.trim() || null,
  checked,
});

/** The API's 400 errors go under their field; anything else is a banner (root error). */
export function toItemErrors(
  error: unknown,
  itemForm: FieldTree<ItemFormValue>,
): WithOptionalFieldTree<ValidationError>[] | ValidationError {
  const errors: Record<string, string[]> | undefined =
    error instanceof HttpErrorResponse && error.status === 400
      ? error.error?.errors
      : undefined;
  if (!errors) {
    return { kind: 'saveFailed' };
  }
  const fields: Record<string, FieldTree<unknown>> = {
    Name: itemForm.name,
    Quantity: itemForm.quantity,
    Unit: itemForm.unit,
  };
  return Object.entries(errors).flatMap(([path, messages]) =>
    messages.map((message) => ({
      kind: 'server',
      message,
      fieldTree: fields[path],
    })),
  );
}
