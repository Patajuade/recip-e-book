import { Component, computed, input, model } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { TranslocoDirective } from '@jsverse/transloco';
import { TagSummary } from '@recip-e-book/web-recipes-models';

/** Picks tag keys from the catalog; bind it with `[formField]` like an input. */
@Component({
  selector: 'lib-tag-picker',
  imports: [TranslocoDirective],
  templateUrl: './tag-picker.html',
  styleUrl: './tag-picker.css',
})
export class TagPicker implements FormValueControl<string[]> {
  readonly value = model<string[]>([]);
  readonly catalog = input.required<readonly TagSummary[]>();

  protected readonly selected = computed(() => new Set(this.value()));
  protected readonly categoryOf = computed(
    () => new Map(this.catalog().map((tag) => [tag.key, tag.category])),
  );

  // Catalog order is kept: the API returns tags grouped by category.
  protected readonly groups = computed(() => {
    const groups = new Map<string, string[]>();
    for (const tag of this.catalog()) {
      groups.set(tag.category, [...(groups.get(tag.category) ?? []), tag.key]);
    }
    return [...groups].map(([category, keys]) => ({
      category,
      keys,
      selectedCount: keys.filter((key) => this.selected().has(key)).length,
    }));
  });

  protected toggle(key: string): void {
    this.value.update((keys) =>
      keys.includes(key) ? keys.filter((k) => k !== key) : [...keys, key],
    );
  }
}
