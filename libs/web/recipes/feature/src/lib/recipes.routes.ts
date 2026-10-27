import { Routes } from '@angular/router';
import { marker } from '@jsverse/transloco-keys-manager/marker';
import { RecipeCook } from './recipe-cook/recipe-cook';
import { RecipeDetail } from './recipe-detail/recipe-detail';
import { RecipeEdit } from './recipe-edit/recipe-edit';
import { unsavedChangesGuard } from './recipe-edit/unsaved-changes-guard';
import { RecipeList } from './recipe-list/recipe-list';

export const recipesRoutes: Routes = [
  {
    path: '',
    component: RecipeList,
    title: marker('recipes.list.pageTitle'),
  },
  {
    path: 'new',
    component: RecipeEdit,
    title: marker('recipes.new.pageTitle'),
    canDeactivate: [unsavedChangesGuard],
  },
  {
    path: ':id',
    component: RecipeDetail,
    title: marker('recipes.detail.pageTitle'),
  },
  {
    path: ':id/cook',
    component: RecipeCook,
    title: marker('recipes.cook.pageTitle'),
  },
  {
    path: ':id/edit',
    component: RecipeEdit,
    title: marker('recipes.edit.pageTitle'),
    canDeactivate: [unsavedChangesGuard],
  },
];
