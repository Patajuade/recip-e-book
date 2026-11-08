import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { safeReturnUrl, signedInGuard } from './account-guards';
import { AccountSession } from './account-session';

describe('signedInGuard', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });
  });

  async function run(answer: object | 'offline') {
    const session = TestBed.inject(AccountSession);
    const request = TestBed.inject(HttpTestingController).expectOne(
      '/api/auth/session',
    );
    if (answer === 'offline') {
      request.error(new ProgressEvent('error'), { status: 0 });
    } else {
      request.flush(answer);
    }
    await session.ready;
    return (await TestBed.runInInjectionContext(() =>
      signedInGuard(
        undefined as never,
        {
          url: '/recipes/42',
        } as RouterStateSnapshot,
      ),
    )) as boolean | UrlTree;
  }

  it('sends a visitor to sign in, with the page to come back to', async () => {
    const result = await run({ email: null, registrationOpen: false });

    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
      '/account/sign-in?returnUrl=%2Frecipes%2F42',
    );
  });

  it('lets someone signed in through', async () => {
    expect(await run({ email: 'a@example.com', registrationOpen: false })).toBe(
      true,
    );
  });

  it('lets the page open offline, when the session is unknown', async () => {
    expect(await run('offline')).toBe(true);
  });
});

describe('safeReturnUrl', () => {
  it('keeps a path of the app', () => {
    expect(safeReturnUrl('/shopping/7?x=1')).toBe('/shopping/7?x=1');
  });

  it('never leads to another site', () => {
    for (const url of [
      '//evil.example',
      '/\\evil.example',
      'https://evil.example',
      'javascript:alert(1)',
      '',
      undefined,
    ]) {
      expect(safeReturnUrl(url)).toBe('/recipes');
    }
  });
});
