import { NgTemplateOutlet } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { form, FormField, FormRoot } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import {
  ShoppingListApi,
  shoppingListResource,
} from '@recip-e-book/web-shopping-data-access';
import { ShoppingListItem } from '@recip-e-book/web-shopping-models';
import { ShoppingItem } from '@recip-e-book/web-shopping-ui';
import {
  Confirmation,
  LabeledField,
  Toaster,
} from '@recip-e-book/web-shared-ui';
import {
  emptyItem,
  ItemFormValue,
  itemFormSchema,
  toItemFieldErrors,
  toItemFormValue,
  toItemInput,
} from './item-form';

/** One shopping list, used in the store: check items off, add, edit or remove them. */
@Component({
  selector: 'lib-shopping-list',
  imports: [
    NgTemplateOutlet,
    TranslocoDirective,
    RouterLink,
    FormField,
    FormRoot,
    LabeledField,
    ShoppingItem,
  ],
  templateUrl: './shopping-list.html',
  styleUrl: './shopping-list.css',
})
export class ShoppingList {
  readonly id = input.required<string>();

  protected readonly list = shoppingListResource(this.id);

  protected readonly notFound = computed(() => {
    const error = this.list.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  private readonly items = computed(() =>
    this.list.hasValue() ? this.list.value().items : [],
  );
  // What's left to buy first; checked items go below, in the cart.
  protected readonly toBuy = computed(() =>
    this.items().filter((item) => !item.checked),
  );
  protected readonly inCart = computed(() =>
    this.items().filter((item) => item.checked),
  );

  private readonly api = inject(ShoppingListApi);
  private readonly confirmation = inject(Confirmation);
  private readonly toaster = inject(Toaster);
  private readonly transloco = inject(TranslocoService);

  // Failures that aren't about a field. Not submission errors: those block submit()
  // until a value changes, so a plain retry would do nothing.
  protected readonly addFailed = signal(false);
  protected readonly editFailed = signal(false);

  protected readonly newItem = signal<ItemFormValue>(emptyItem());
  protected readonly newItemForm = form(this.newItem, itemFormSchema, {
    submission: {
      action: async (newItemForm) => {
        this.addFailed.set(false);
        try {
          await this.api.addItem(this.id(), toItemInput(this.newItem(), false));
        } catch (error) {
          const fieldErrors = toItemFieldErrors(error, newItemForm);
          this.addFailed.set(fieldErrors.length === 0);
          return fieldErrors;
        }
        this.newItemForm().reset(emptyItem());
        this.list.reload();
        return undefined;
      },
    },
  });

  /** The item being edited inline, if any. */
  protected readonly editing = signal<ShoppingListItem | null>(null);
  protected readonly edit = signal<ItemFormValue>(emptyItem());
  protected readonly editForm = form(this.edit, itemFormSchema, {
    submission: {
      action: async (editForm) => {
        const item = this.editing();
        if (!item) {
          return undefined;
        }
        this.editFailed.set(false);
        try {
          await this.api.updateItem(
            this.id(),
            item.id,
            toItemInput(this.edit(), item.checked),
          );
        } catch (error) {
          const fieldErrors = toItemFieldErrors(error, editForm);
          this.editFailed.set(fieldErrors.length === 0);
          return fieldErrors;
        }
        this.editing.set(null);
        this.list.reload();
        return undefined;
      },
    },
  });

  protected startEdit(item: ShoppingListItem): void {
    this.editFailed.set(false);
    this.editForm().reset(toItemFormValue(item));
    this.editing.set(item);
  }

  /**
   * Checked at once on screen, then saved: a tap in the store doesn't wait for the network.
   * If saving fails, the list is reloaded from the server (the error toast says why).
   */
  protected async toggle(item: ShoppingListItem): Promise<void> {
    const checked = !item.checked;
    this.list.update((list) =>
      list
        ? {
            ...list,
            items: list.items.map((i) =>
              i.id === item.id ? { ...i, checked } : i,
            ),
          }
        : list,
    );
    try {
      await this.api.updateItem(
        this.id(),
        item.id,
        toItemInput(toItemFormValue(item), checked),
      );
    } catch {
      this.list.reload();
    }
  }

  protected async remove(item: ShoppingListItem): Promise<void> {
    this.list.update((list) =>
      list
        ? { ...list, items: list.items.filter((i) => i.id !== item.id) }
        : list,
    );
    try {
      await this.api.deleteItem(this.id(), item.id);
    } catch {
      this.list.reload();
    }
  }

  protected async clearChecked(): Promise<void> {
    const confirmed = await this.confirmation.ask({
      title: this.transloco.translate('shopping.list.clearTitle'),
      message: this.transloco.translate('shopping.list.clearMessage', {
        count: this.inCart().length,
      }),
      confirmLabel: this.transloco.translate('shopping.list.clearConfirm'),
    });
    if (!confirmed) {
      return;
    }
    try {
      await this.api.clearChecked(this.id());
    } catch {
      this.list.reload();
      return;
    }
    this.toaster.show(this.transloco.translate('shopping.list.cleared'));
    this.list.reload();
  }
}
