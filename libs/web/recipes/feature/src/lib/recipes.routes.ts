import { Routes } from '@angular/router';
import { RecipeDetail } from './recipe-detail/recipe-detail';
import { RecipeEdit } from './recipe-edit/recipe-edit';
import { RecipeList } from './recipe-list/recipe-list';

export const recipesRoutes: Routes = [
  {
    path: '',
    component: RecipeList,
    title: $localize`:@@recipes.list.pageTitle:Recipes`,
  },
  {
    path: 'new',
    component: RecipeEdit,
    title: $localize`:@@recipes.new.pageTitle:New recipe`,
  },
  {
    path: ':id',
    component: RecipeDetail,
    title: $localize`:@@recipes.detail.pageTitle:Recipe`,
  },
  {
    path: ':id/edit',
    component: RecipeEdit,
    title: $localize`:@@recipes.edit.pageTitle:Edit recipe`,
  },
];
