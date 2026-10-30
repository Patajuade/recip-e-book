import { Routes } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';
import { unsavedChangesGuard } from './recipe-edit/unsaved-changes-guard';

// One lazy chunk per page: the list doesn't bring the form, its dialog or cooking mode.
export const recipesRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./recipe-list/recipe-list').then((m) => m.RecipeList),
    title: marker('recipes.list.pageTitle'),
  },
  {
    path: 'new',
    loadComponent: () =>
      import('./recipe-edit/recipe-edit').then((m) => m.RecipeEdit),
    title: marker('recipes.new.pageTitle'),
    canDeactivate: [unsavedChangesGuard],
  },
  {
    path: ':id',
    loadComponent: () =>
      import('./recipe-detail/recipe-detail').then((m) => m.RecipeDetail),
    title: marker('recipes.detail.pageTitle'),
  },
  {
    path: ':id/cook',
    loadComponent: () =>
      import('./recipe-cook/recipe-cook').then((m) => m.RecipeCook),
    title: marker('recipes.cook.pageTitle'),
  },
  {
    path: ':id/edit',
    loadComponent: () =>
      import('./recipe-edit/recipe-edit').then((m) => m.RecipeEdit),
    title: marker('recipes.edit.pageTitle'),
    canDeactivate: [unsavedChangesGuard],
  },
];
