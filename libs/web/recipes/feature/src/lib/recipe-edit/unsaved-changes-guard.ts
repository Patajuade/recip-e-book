import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { TranslocoService } from '@jsverse/transloco';
import { Confirmation } from '@recip-e-book/web-shared-ui';
import { RecipeEdit } from './recipe-edit';

/** Asks before leaving the form with unsaved changes (navigation inside the app only). */
export const unsavedChangesGuard: CanDeactivateFn<RecipeEdit> = (component) => {
  if (!component.hasUnsavedChanges()) {
    return true;
  }
  const transloco = inject(TranslocoService);
  return inject(Confirmation).ask({
    title: transloco.translate('recipes.form.leaveTitle'),
    message: transloco.translate('recipes.form.leaveMessage'),
    confirmLabel: transloco.translate('recipes.form.leave'),
  });
};
