import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { RecipeInput } from '@recip-e-book/web-recipes-models';
import { firstValueFrom } from 'rxjs';

/** Recipe commands (writes). Reads go through the resources in recipe-resources.ts. */
@Service()
export class RecipeApi {
  private readonly http = inject(HttpClient);

  async create(recipe: RecipeInput): Promise<string> {
    const { id } = await firstValueFrom(
      this.http.post<{ id: string }>('/api/recipes', recipe),
    );
    return id;
  }

  async update(id: string, recipe: RecipeInput): Promise<void> {
    await firstValueFrom(
      this.http.put(`/api/recipes/${encodeURIComponent(id)}`, recipe),
    );
  }
}
