import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Service, signal } from '@angular/core';
import type { components } from '@recip-e-book/web-shared-api-types';
import { firstValueFrom } from 'rxjs';

type Schemas = components['schemas'];
export type Session = Schemas['Session'];
export type RegisterInput = Schemas['RegisterInput'];
export type LoginInput = Schemas['LoginInput'];

const AUTH = '/api/auth';

// The account the service worker's cached API responses belong to.
const CACHE_OWNER_KEY = 'recip-e-book.cache-owner';

/** Who is signed in, and the commands that change it. */
@Service()
export class AccountSession {
  private readonly http = inject(HttpClient);

  // Null while unknown: no answer yet, or none possible (offline).
  private readonly session = signal<Session | null>(null);

  readonly known = computed(() => this.session() !== null);
  readonly email = computed(() => this.session()?.email ?? null);
  readonly signedIn = computed(() => this.email() !== null);
  readonly registrationOpen = computed(
    () => this.session()?.registrationOpen ?? false,
  );

  /** Settles when the first answer came, or could not come. */
  readonly ready = this.refresh();

  /**
   * Reads the session, which also writes the antiforgery cookie. Past the service worker: an
   * old answer could say someone is signed in after they signed out.
   */
  async refresh(): Promise<void> {
    let session: Session;
    try {
      session = await firstValueFrom(
        this.http.get<Session>(`${AUTH}/session`, {
          headers: { 'ngsw-bypass': '' },
        }),
      );
    } catch {
      // Keeps what is known; offline, the app stays usable.
      return;
    }
    await this.keepCacheFor(session.email);
    this.session.set(session);
  }

  async signIn(input: LoginInput): Promise<void> {
    await this.post('login', input);
    await this.afterChange();
  }

  async signUp(input: RegisterInput): Promise<void> {
    await this.post('register', input);
    await this.afterChange();
  }

  async signOut(): Promise<void> {
    await this.post('logout', {});
    await this.afterChange();
  }

  // The token depends on the user.
  private async afterChange(): Promise<void> {
    await this.refresh();
  }

  /**
   * The service worker answers from its cache when the network fails or is slow, whoever
   * asks. So when the session shows someone else than the cache's owner, or nobody (signed
   * out, or a session that expired), the cache is emptied before any page reads it: the
   * guards wait for the first answer.
   */
  private async keepCacheFor(email: string | null): Promise<void> {
    if (readCacheOwner() === email) {
      return;
    }
    await clearCachedApiResponses();
    writeCacheOwner(email);
  }

  private async post(path: string, body: unknown): Promise<void> {
    await this.ready;
    const send = () =>
      firstValueFrom(this.http.post<void>(`${AUTH}/${path}`, body));
    try {
      await send();
    } catch (error) {
      if (!isAntiforgeryFailure(error)) {
        throw error;
      }
      // The token was for another user, such as a session that expired meanwhile.
      await this.refresh();
      await send();
    }
  }
}

// Storage can be unavailable (private mode, blocked site data): the cache is then always emptied.
function readCacheOwner(): string | null | undefined {
  try {
    const value = localStorage.getItem(CACHE_OWNER_KEY);
    return value === null ? undefined : (JSON.parse(value) as string | null);
  } catch {
    return undefined;
  }
}

function writeCacheOwner(email: string | null): void {
  try {
    localStorage.setItem(CACHE_OWNER_KEY, JSON.stringify(email));
  } catch {
    // See readCacheOwner.
  }
}

// A 400 from validation lists its `errors`; the antiforgery check's doesn't.
const isAntiforgeryFailure = (error: unknown): boolean =>
  error instanceof HttpErrorResponse &&
  error.status === 400 &&
  !error.error?.errors;

/**
 * Removes the API responses the service worker kept for offline reading, so nobody else can
 * read them on a shared device. By URL, not by cache: the names of its caches aren't part of
 * Angular's API.
 */
async function clearCachedApiResponses(): Promise<void> {
  if (typeof caches === 'undefined') {
    return;
  }
  for (const name of await caches.keys()) {
    const cache = await caches.open(name);
    for (const request of await cache.keys()) {
      if (new URL(request.url).pathname.startsWith('/api/')) {
        await cache.delete(request);
      }
    }
  }
}
