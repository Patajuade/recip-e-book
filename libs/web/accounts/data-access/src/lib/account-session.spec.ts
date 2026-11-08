import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { visitorOnlyGuard } from './account-guards';
import { AccountSession, Session } from './account-session';

const SESSION = '/api/auth/session';
const visitor = { email: null, registrationOpen: true };
const signedIn = { email: 'a@example.com', registrationOpen: false };
const credentials = { email: 'a@example.com', password: 'a long passphrase' };

describe('AccountSession', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    vi.unstubAllGlobals();
  });

  async function start(session: Session = visitor) {
    const account = TestBed.inject(AccountSession);
    http.expectOne(SESSION).flush(session);
    await account.ready;
    return account;
  }

  it('is unknown until the session answers', async () => {
    const account = TestBed.inject(AccountSession);
    expect(account.known()).toBe(false);

    http.expectOne(SESSION).flush(signedIn);
    await account.ready;

    expect(account.known()).toBe(true);
    expect(account.signedIn()).toBe(true);
    expect(account.email()).toBe('a@example.com');
    expect(account.registrationOpen()).toBe(false);
  });

  it('stays unknown and usable when the session cannot be read', async () => {
    const account = TestBed.inject(AccountSession);

    http.expectOne(SESSION).error(new ProgressEvent('error'), { status: 0 });
    await account.ready;

    expect(account.known()).toBe(false);
    expect(account.signedIn()).toBe(false);
  });

  it('reads the session past the service worker', () => {
    TestBed.inject(AccountSession);

    const request = http.expectOne(SESSION);

    expect(request.request.headers.has('ngsw-bypass')).toBe(true);
    request.flush(visitor);
  });

  it('reads the session again after signing in, for the new token', async () => {
    const account = await start();

    const signingIn = account.signIn(credentials);
    (await respondTo('/api/auth/login')).flush(null);
    (await respondTo(SESSION)).flush(signedIn);
    await signingIn;

    expect(account.email()).toBe('a@example.com');
  });

  it('retries once after an antiforgery failure, with a new token', async () => {
    const account = await start();

    const signingIn = account.signIn(credentials);
    (await respondTo('/api/auth/login')).flush(
      { detail: 'The request could not be verified.' },
      { status: 400, statusText: 'Bad Request' },
    );
    (await respondTo(SESSION)).flush(visitor);
    (await respondTo('/api/auth/login')).flush(null);
    (await respondTo(SESSION)).flush(signedIn);
    await signingIn;

    expect(account.signedIn()).toBe(true);
  });

  it('passes on a validation failure without retrying', async () => {
    const account = await start();

    const signingUp = account.signUp(credentials);
    (await respondTo('/api/auth/register')).flush(
      { errors: { Email: ['Already used.'] } },
      { status: 400, statusText: 'Bad Request' },
    );

    await expect(signingUp).rejects.toMatchObject({ status: 400 });
  });

  it('removes the cached API responses when signing out, and nothing else', async () => {
    const entries = [
      new Request('https://app.test/api/recipes?limit=20'),
      new Request('https://app.test/main.js'),
    ];
    const cache = {
      keys: vi.fn(async () => entries),
      delete: vi.fn(async () => true),
    };
    vi.stubGlobal('caches', {
      keys: async () => ['ngsw:data'],
      open: async () => cache,
    });
    const account = await start(signedIn);

    const signingOut = account.signOut();
    (await respondTo('/api/auth/logout')).flush(null);
    (await respondTo(SESSION)).flush(visitor);
    await signingOut;

    expect(cache.delete).toHaveBeenCalledTimes(1);
    expect(cache.delete).toHaveBeenCalledWith(entries[0]);
    expect(account.signedIn()).toBe(false);
  });

  describe('visitorOnlyGuard', () => {
    const run = () =>
      TestBed.runInInjectionContext(() =>
        visitorOnlyGuard(undefined as never, undefined as never),
      ) as Promise<boolean | UrlTree>;

    it('sends someone signed in to the recipes', async () => {
      await start(signedIn);

      const result = await run();

      expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
        '/recipes',
      );
    });

    it('lets a visitor through, and when the session is unknown', async () => {
      await start(visitor);

      expect(await run()).toBe(true);
    });
  });

  // Awaits until the service has sent that request.
  async function respondTo(url: string) {
    let request: ReturnType<HttpTestingController['expectOne']> | undefined;
    await vi.waitFor(() => {
      const [match] = http.match(url);
      expect(match).toBeDefined();
      request = match;
    });
    return request as ReturnType<HttpTestingController['expectOne']>;
  }
});
