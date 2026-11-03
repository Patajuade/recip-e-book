import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Page, pagedResource } from './paged-resource';

describe('pagedResource', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function setup() {
    const search = signal('');
    const list = TestBed.runInInjectionContext(() =>
      pagedResource<string>(() => ({
        url: '/api/things',
        params: search() ? { search: search() } : undefined,
      })),
    );
    return { search, list };
  }

  // httpResource sends its request when its effect runs, and settles after the response.
  async function respond(url: string, page: Page<string>) {
    TestBed.tick();
    http.expectOne(url).flush(page);
    await TestBed.inject(ApplicationRef).whenStable();
  }

  it('appends the next page, sent with the cursor of the last one', async () => {
    const { list } = setup();
    await respond('/api/things', { items: ['a', 'b'], nextCursor: 'c1' });

    const loading = list.loadMore();
    http
      .expectOne('/api/things?cursor=c1')
      .flush({ items: ['c'], nextCursor: null });
    await loading;

    expect(list.items()).toEqual(['a', 'b', 'c']);
    expect(list.hasMore()).toBe(false);
  });

  it('starts over from the first page when the request changes', async () => {
    const { search, list } = setup();
    await respond('/api/things', { items: ['a'], nextCursor: 'c1' });
    const loading = list.loadMore();
    http
      .expectOne('/api/things?cursor=c1')
      .flush({ items: ['b'], nextCursor: null });
    await loading;

    search.set('x');
    await respond('/api/things?search=x', { items: ['x'], nextCursor: null });

    expect(list.items()).toEqual(['x']);
  });

  it('drops a page that arrives after the list was reloaded', async () => {
    const { search, list } = setup();
    await respond('/api/things', { items: ['a'], nextCursor: 'c1' });

    const loading = list.loadMore();
    const next = http.expectOne('/api/things?cursor=c1');
    search.set('x');
    await respond('/api/things?search=x', { items: ['x'], nextCursor: null });
    next.flush({ items: ['b'], nextCursor: null });
    await loading;

    expect(list.items()).toEqual(['x']);
  });

  it('flags a failed page and lets it be loaded again', async () => {
    const { list } = setup();
    await respond('/api/things', { items: ['a'], nextCursor: 'c1' });

    let loading = list.loadMore();
    http
      .expectOne('/api/things?cursor=c1')
      .flush(null, { status: 500, statusText: 'Server Error' });
    await loading;
    expect(list.loadMoreFailed()).toBe(true);
    expect(list.items()).toEqual(['a']);

    loading = list.loadMore();
    http
      .expectOne('/api/things?cursor=c1')
      .flush({ items: ['b'], nextCursor: null });
    await loading;
    expect(list.loadMoreFailed()).toBe(false);
    expect(list.items()).toEqual(['a', 'b']);
  });

  it('does nothing on the last page', async () => {
    const { list } = setup();
    await respond('/api/things', { items: ['a'], nextCursor: null });

    await list.loadMore();

    http.expectNone((request) => request.url === '/api/things');
    expect(list.items()).toEqual(['a']);
  });
});
