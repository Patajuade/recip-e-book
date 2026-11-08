import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { firstValueFrom } from 'rxjs';
import { getTranslocoModule } from '../testing/transloco-testing';
import { httpErrorInterceptor } from './http-error-interceptor';

@Component({ template: '' })
class Blank {}

describe('httpErrorInterceptor', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [getTranslocoModule()],
      providers: [
        provideHttpClient(withInterceptors([httpErrorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([
          { path: 'account/sign-in', component: Blank },
          { path: 'recipes', component: Blank },
        ]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  it('lets the session be read at start, through it', () => {
    // AccountSession's first request goes through the interceptor: resolving AccountSession
    // there right away would be a circular dependency, and the request would never leave.
    TestBed.inject(AccountSession);

    http
      .expectOne('/api/auth/session')
      .flush({ email: null, registrationOpen: false });
  });

  it('sends to signing in on a 401, and back to the page afterwards', async () => {
    const router = TestBed.inject(Router);
    const session = TestBed.inject(AccountSession);
    http
      .expectOne('/api/auth/session')
      .flush({ email: 'a@example.com', registrationOpen: false });
    await session.ready;
    await router.navigateByUrl('/recipes');

    const read = firstValueFrom(TestBed.inject(HttpClient).get('/api/recipes'));
    http
      .expectOne('/api/recipes')
      .flush(null, { status: 401, statusText: 'Unauthorized' });
    await expect(read).rejects.toMatchObject({ status: 401 });
    http
      .expectOne('/api/auth/session')
      .flush({ email: null, registrationOpen: false });

    await vi.waitFor(() =>
      expect(router.url).toBe('/account/sign-in?returnUrl=%2Frecipes'),
    );
    expect(session.signedIn()).toBe(false);
  });
});
