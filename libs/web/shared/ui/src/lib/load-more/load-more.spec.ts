import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { LoadMore } from './load-more';

@Component({
  imports: [LoadMore],
  template: `
    <ul #list>
      @for (item of items(); track item) {
        <li>
          <a href="#">{{ item }}</a>
        </li>
      }
    </ul>
    <lib-load-more
      [list]="list"
      [hasMore]="hasMore()"
      [loading]="loading()"
      [failed]="failed()"
      (loadMore)="loads = loads + 1"
    />
  `,
})
class Host {
  readonly items = signal(['a', 'b']);
  readonly hasMore = signal(true);
  readonly loading = signal(false);
  readonly failed = signal(false);
  loads = 0;
}

describe('LoadMore', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TranslocoTestingModule.forRoot({
          langs: {
            en: {
              loadMore: {
                button: 'Load more',
                loading: 'Loading…',
                failed: "Couldn't load more.",
              },
            },
          },
          translocoConfig: { availableLangs: ['en'], defaultLang: 'en' },
          preloadLangs: true,
        }),
      ],
    });
  });

  async function setup() {
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    const host = fixture.componentInstance;
    const element = fixture.nativeElement as HTMLElement;
    const button = () => element.querySelector('lib-load-more button');
    // What the page does: loading, the new items, loaded.
    const load = async (newItems: string[], failed = false) => {
      host.loading.set(true);
      await fixture.whenStable();
      host.items.update((items) => [...items, ...newItems]);
      host.failed.set(failed);
      host.loading.set(false);
      await fixture.whenStable();
    };
    return { fixture, host, button, load };
  }

  it('moves the focus to the first new item after a click', async () => {
    const { host, button, load } = await setup();

    (button() as HTMLButtonElement).click();
    expect(host.loads).toBe(1);
    await load(['c', 'd']);

    expect(document.activeElement?.textContent?.trim()).toBe('c');
  });

  it('keeps the focus on the button when the page failed', async () => {
    const { button, load } = await setup();

    const loadMore = button() as HTMLButtonElement;
    loadMore.focus();
    loadMore.click();
    await load([], true);

    expect(document.activeElement).toBe(button());
    expect(
      document.querySelector('lib-load-more [role=alert]')?.textContent,
    ).toContain("Couldn't load more.");
  });

  it('ignores clicks while a page loads', async () => {
    const { fixture, host, button } = await setup();

    host.loading.set(true);
    await fixture.whenStable();
    (button() as HTMLButtonElement).click();

    expect(host.loads).toBe(0);
    expect(button()?.textContent?.trim()).toBe('Loading…');
  });

  it('has no button on the last page', async () => {
    const { fixture, host, button } = await setup();

    host.hasMore.set(false);
    await fixture.whenStable();

    expect(button()).toBeNull();
  });
});
