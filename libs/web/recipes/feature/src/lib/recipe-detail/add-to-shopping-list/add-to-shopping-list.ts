import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { HttpErrorResponse } from '@angular/common/http';
import {
  afterNextRender,
  Component,
  ElementRef,
  inject,
  Injector,
  linkedSignal,
  signal,
  viewChild,
} from '@angular/core';
import {
  FieldTree,
  form,
  FormField,
  FormRoot,
  max,
  maxLength,
  min,
  required,
  schema,
  ValidationError,
  WithOptionalFieldTree,
} from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { TranslocoDirective } from '@jsverse/transloco';
import {
  ShoppingListApi,
  shoppingListsResource,
} from '@recip-e-book/web-shopping-data-access';
import { LabeledField } from '@recip-e-book/web-shared-ui';

export interface AddToShoppingListData {
  recipeId: string;
  recipeTitle: string;
  /** The recipe's servings; null when unknown (quantities can't be scaled). */
  servings: number | null;
}

/** Radio value of the "New list" choice; other values are list ids. */
const NEW_LIST = 'new';

interface AddToListValue {
  target: string;
  newName: string;
  servings: number | null;
}

// Mirrors the API rules (ShoppingListInput, AddRecipeInput).
const addToListSchema = schema<AddToListValue>((value) => {
  required(value.newName, {
    when: ({ valueOf }) => valueOf(value.target) === NEW_LIST,
  });
  maxLength(value.newName, 100);
  min(value.servings, 1);
  max(value.servings, 100);
});

interface Added {
  listId: string;
  listName: string;
  count: number;
}

/** Content of the dialog that copies a recipe's ingredients into a shopping list. */
@Component({
  selector: 'lib-add-to-shopping-list',
  imports: [TranslocoDirective, RouterLink, FormField, FormRoot, LabeledField],
  templateUrl: './add-to-shopping-list.html',
  styleUrl: './add-to-shopping-list.css',
  host: {
    class:
      'flex max-h-[calc(100dvh-2rem)] w-[min(28rem,calc(100vw-2rem))] flex-col gap-4 overflow-y-auto rounded-lg border border-line bg-surface p-6 shadow-xl',
  },
})
export class AddToShoppingList {
  protected readonly data = inject<AddToShoppingListData>(DIALOG_DATA);
  protected readonly dialogRef = inject(DialogRef);
  protected readonly newList = NEW_LIST;

  protected readonly lists = shoppingListsResource();

  private readonly api = inject(ShoppingListApi);
  private readonly injector = inject(Injector);

  protected readonly value = linkedSignal<string | undefined, AddToListValue>({
    // Defaults to the first list once they are loaded; later reloads keep the user's choice.
    source: () =>
      this.lists.hasValue()
        ? (this.lists.value()[0]?.id ?? NEW_LIST)
        : undefined,
    computation: (defaultTarget, previous) =>
      previous?.source !== undefined
        ? previous.value
        : {
            target: defaultTarget ?? NEW_LIST,
            newName: '',
            servings: this.data.servings,
          },
  });

  protected readonly added = signal<Added | null>(null);
  private readonly addedMessage =
    viewChild<ElementRef<HTMLElement>>('addedMessage');

  /**
   * A failure that isn't about a field (server down, list deleted…). Kept out of the form's
   * submission errors: those make the form invalid until a value changes, which would block a
   * plain retry.
   */
  protected readonly failed = signal(false);

  protected readonly addForm = form(this.value, addToListSchema, {
    submission: {
      action: async (addForm) => {
        this.failed.set(false);
        const { target, newName, servings } = this.value();
        let listId = target;
        let listName = this.lists.hasValue()
          ? this.lists.value().find((list) => list.id === target)?.name
          : undefined;
        try {
          if (target === NEW_LIST) {
            listName = newName.trim();
            listId = await this.api.create({ name: newName });
            // A retry after a failed add must reuse this list, not create a second one.
            this.value.update((v) => ({ ...v, target: listId, newName: '' }));
            this.lists.reload();
          }
          const count = await this.api.addRecipe(listId, {
            recipeId: this.data.recipeId,
            servings,
          });
          this.added.set({ listId, listName: listName ?? '', count });
        } catch (error) {
          if (error instanceof HttpErrorResponse && error.status === 404) {
            // The list was deleted meanwhile: show the current lists again.
            this.lists.reload();
          }
          const fieldErrors = toFieldErrors(error, addForm);
          this.failed.set(fieldErrors.length === 0);
          return fieldErrors;
        }
        // The submit button is gone: move the focus to the outcome so it is announced.
        afterNextRender(() => this.addedMessage()?.nativeElement.focus(), {
          injector: this.injector,
        });
        return undefined;
      },
    },
  });
}

/** The API's 400 errors on Name and Servings, placed under their field. */
function toFieldErrors(
  error: unknown,
  addForm: FieldTree<AddToListValue>,
): WithOptionalFieldTree<ValidationError>[] {
  const errors: Record<string, string[]> | undefined =
    error instanceof HttpErrorResponse && error.status === 400
      ? error.error?.errors
      : undefined;
  const fields: Record<string, FieldTree<unknown>> = {
    Name: addForm.newName,
    Servings: addForm.servings,
  };
  return Object.entries(errors ?? {})
    .filter(([path]) => path in fields)
    .flatMap(([path, messages]) =>
      messages.map((message) => ({
        kind: 'server',
        message,
        fieldTree: fields[path],
      })),
    );
}
