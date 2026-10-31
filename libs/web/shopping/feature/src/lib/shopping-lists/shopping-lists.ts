import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import {
  FieldTree,
  form,
  FormField,
  FormRoot,
  maxLength,
  required,
  schema,
  ValidationError,
  WithOptionalFieldTree,
} from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { TranslocoDirective, TranslocoService } from '@jsverse/transloco';
import {
  ShoppingListApi,
  shoppingListsResource,
} from '@recip-e-book/web-shopping-data-access';
import { ShoppingListSummary } from '@recip-e-book/web-shopping-models';
import {
  Confirmation,
  LabeledField,
  Toaster,
} from '@recip-e-book/web-shared-ui';
import { delayedLoading } from '@recip-e-book/web-shared-util';

interface ListName {
  name: string;
}

// Mirrors the API rules (ShoppingListInput): required, 100 characters at most.
const listNameSchema = schema<ListName>((list) => {
  required(list.name);
  maxLength(list.name, 100);
});

/** The shopping lists: create, rename inline, delete after a confirmation. */
@Component({
  selector: 'lib-shopping-lists',
  imports: [TranslocoDirective, RouterLink, FormField, FormRoot, LabeledField],
  templateUrl: './shopping-lists.html',
  styleUrl: './shopping-lists.css',
})
export class ShoppingLists {
  protected readonly lists = shoppingListsResource();
  protected readonly waiting = computed(
    () => this.lists.isLoading() && !this.lists.hasValue(),
  );
  protected readonly skeleton = delayedLoading(this.waiting);

  private readonly api = inject(ShoppingListApi);
  private readonly confirmation = inject(Confirmation);
  private readonly toaster = inject(Toaster);
  private readonly transloco = inject(TranslocoService);

  // Failures that aren't about the name. Not submission errors: those block submit()
  // until a value changes, so a plain retry would do nothing.
  protected readonly createFailed = signal(false);
  protected readonly renameFailed = signal(false);

  protected readonly newList = signal<ListName>({ name: '' });
  protected readonly newListForm = form(this.newList, listNameSchema, {
    submission: {
      action: async (newListForm) => {
        this.createFailed.set(false);
        try {
          await this.api.create({ name: this.newList().name });
        } catch (error) {
          const fieldErrors = nameErrors(error, newListForm);
          this.createFailed.set(fieldErrors.length === 0);
          return fieldErrors;
        }
        this.newListForm().reset({ name: '' });
        this.lists.reload();
        return undefined;
      },
    },
  });

  /** Id of the list being renamed, if any. */
  protected readonly renaming = signal<string | null>(null);
  protected readonly rename = signal<ListName>({ name: '' });
  protected readonly renameForm = form(this.rename, listNameSchema, {
    submission: {
      action: async (renameForm) => {
        const id = this.renaming();
        if (!id) {
          return undefined;
        }
        this.renameFailed.set(false);
        try {
          await this.api.rename(id, { name: this.rename().name });
        } catch (error) {
          const fieldErrors = nameErrors(error, renameForm);
          this.renameFailed.set(fieldErrors.length === 0);
          return fieldErrors;
        }
        this.renaming.set(null);
        this.lists.reload();
        return undefined;
      },
    },
  });

  protected startRename(list: ShoppingListSummary): void {
    this.renameFailed.set(false);
    this.renameForm().reset({ name: list.name });
    this.renaming.set(list.id);
  }

  protected async delete(list: ShoppingListSummary): Promise<void> {
    const confirmed = await this.confirmation.ask({
      title: this.transloco.translate('shopping.lists.deleteTitle'),
      message: this.transloco.translate('shopping.lists.deleteMessage', {
        name: list.name,
      }),
      confirmLabel: this.transloco.translate('shopping.lists.delete'),
    });
    if (!confirmed) {
      return;
    }
    try {
      await this.api.delete(list.id);
    } catch (error) {
      // A 404 means it is already gone: the outcome the user asked for.
      if (!(error instanceof HttpErrorResponse && error.status === 404)) {
        this.toaster.show(
          this.transloco.translate('shopping.lists.deleteFailed'),
        );
        return;
      }
    }
    this.toaster.show(this.transloco.translate('shopping.lists.deleted'));
    this.lists.reload();
  }
}

/** The API's 400 errors on Name, placed under the field; empty for any other failure. */
function nameErrors(
  error: unknown,
  listForm: FieldTree<ListName>,
): WithOptionalFieldTree<ValidationError>[] {
  const messages: string[] =
    error instanceof HttpErrorResponse && error.status === 400
      ? (error.error?.errors?.Name ?? [])
      : [];
  return messages.map((message) => ({
    kind: 'server',
    message,
    fieldTree: listForm.name,
  }));
}
