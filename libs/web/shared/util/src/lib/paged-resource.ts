import { HttpClient, httpResource } from '@angular/common/http';
import {
  computed,
  inject,
  linkedSignal,
  signal,
  untracked,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';

/** One page of a list, as the API returns it (`Common/Paging.cs`). */
export interface Page<T> {
  items: T[];
  nextCursor: string | null;
}

export interface PagedRequest {
  url: string;
  params?: Record<string, string | number>;
}

/**
 * A list loaded a page at a time. The first page is an `httpResource`, so it loads again when
 * the request changes (filters) or on `reload()`, and the pages added by `loadMore()` are then
 * dropped: the list starts over from its first page. Needs an injection context.
 */
export function pagedResource<T>(request: () => PagedRequest | undefined) {
  const http = inject(HttpClient);
  const first = httpResource<Page<T>>(request);
  const firstPage = computed(() =>
    first.hasValue() ? first.value() : undefined,
  );
  const more = linkedSignal<Page<T> | undefined, Page<T>[]>({
    source: firstPage,
    computation: () => [],
  });
  const lastPage = computed(() => more()[more().length - 1] ?? firstPage());
  const loadingMore = signal(false);
  const loadMoreFailed = signal(false);

  async function loadMore(): Promise<void> {
    const cursor = lastPage()?.nextCursor;
    const current = untracked(request);
    if (!cursor || !current || loadingMore()) {
      return;
    }
    const startedFrom = firstPage();
    loadingMore.set(true);
    loadMoreFailed.set(false);
    try {
      const page = await firstValueFrom(
        http.get<Page<T>>(current.url, {
          params: { ...current.params, cursor },
        }),
      );
      // Reloaded meanwhile (filters, a write): this page belongs to the old list.
      if (firstPage() === startedFrom) {
        more.update((pages) => [...pages, page]);
      }
    } catch {
      // The HTTP error interceptor already shows why in a toast.
      loadMoreFailed.set(true);
    } finally {
      loadingMore.set(false);
    }
  }

  return {
    items: computed(() => [
      ...(firstPage()?.items ?? []),
      ...more().flatMap((page) => page.items),
    ]),
    isLoading: first.isLoading,
    error: first.error,
    hasValue: () => first.hasValue(),
    reload: () => first.reload(),
    hasMore: computed(() => !!lastPage()?.nextCursor),
    loadingMore: loadingMore.asReadonly(),
    loadMoreFailed: loadMoreFailed.asReadonly(),
    loadMore,
  };
}

export type PagedResource<T> = ReturnType<typeof pagedResource<T>>;
