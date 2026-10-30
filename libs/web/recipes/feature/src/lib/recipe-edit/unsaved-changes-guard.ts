import { CanDeactivateFn } from '@angular/router';
// Type only: the routes must not load the form and its dialog before the page is opened.
import type { RecipeEdit } from './recipe-edit';

export const unsavedChangesGuard: CanDeactivateFn<RecipeEdit> = (component) =>
  component.canLeave();
