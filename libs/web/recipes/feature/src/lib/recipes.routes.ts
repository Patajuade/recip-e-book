import { Routes } from '@angular/router';
import { RecipeDetail } from './recipe-detail/recipe-detail';
import { RecipeEdit } from './recipe-edit/recipe-edit';
import { unsavedChangesGuard } from './recipe-edit/unsaved-changes-guard';
import { RecipeList } from './recipe-list/recipe-list';

export const recipesRoutes: Routes = [
  {
    path: '',
    component: RecipeList,
    title: 'recipes.list.pageTitle',
  },
  {
    path: 'new',
    component: RecipeEdit,
    title: 'recipes.new.pageTitle',
    canDeactivate: [unsavedChangesGuard],
  },
  {
    path: ':id',
    component: RecipeDetail,
    title: 'recipes.detail.pageTitle',
  },
  {
    path: ':id/edit',
    component: RecipeEdit,
    title: 'recipes.edit.pageTitle',
    canDeactivate: [unsavedChangesGuard],
  },
];
