import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import {
  AddRecipeInput,
  ShoppingListInput,
  ShoppingListItemInput,
} from '@recip-e-book/web-shopping-models';
import { firstValueFrom } from 'rxjs';

const url = (...segments: string[]) =>
  ['/api/shopping-lists', ...segments.map(encodeURIComponent)].join('/');

/** Shopping list commands (writes). Reads go through shopping-list-resources.ts. */
@Service()
export class ShoppingListApi {
  private readonly http = inject(HttpClient);

  async create(list: ShoppingListInput): Promise<string> {
    const { id } = await firstValueFrom(
      this.http.post<{ id: string }>(url(), list),
    );
    return id;
  }

  async rename(id: string, list: ShoppingListInput): Promise<void> {
    await firstValueFrom(this.http.put(url(id), list));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(url(id)));
  }

  async addItem(listId: string, item: ShoppingListItemInput): Promise<string> {
    const { id } = await firstValueFrom(
      this.http.post<{ id: string }>(url(listId, 'items'), item),
    );
    return id;
  }

  async updateItem(
    listId: string,
    itemId: string,
    item: ShoppingListItemInput,
  ): Promise<void> {
    await firstValueFrom(this.http.put(url(listId, 'items', itemId), item));
  }

  async deleteItem(listId: string, itemId: string): Promise<void> {
    await firstValueFrom(this.http.delete(url(listId, 'items', itemId)));
  }

  async clearChecked(listId: string): Promise<void> {
    await firstValueFrom(this.http.delete(url(listId, 'checked-items')));
  }

  /** Returns how many items were added. */
  async addRecipe(listId: string, recipe: AddRecipeInput): Promise<number> {
    const { itemsAdded } = await firstValueFrom(
      this.http.post<{ itemsAdded: number }>(url(listId, 'recipes'), recipe),
    );
    return itemsAdded;
  }
}
