import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { SignIn } from './sign-in';

@Component({ template: '' })
class Recipes {}

const translations = {
  accounts: {
    email: 'Email',
    password: 'Password',
    signIn: {
      heading: 'Sign in',
      submit: 'Sign in',
      refused: 'Refused',
      unavailable: 'Unavailable',
      noAccount: 'No account',
      createAccount: 'Create one',
    },
  },
  forms: { errors: { required: 'Required', email: 'Bad email' } },
};

describe('SignIn', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<SignIn>;
  let element: HTMLElement;

  async function setup(registrationOpen = false) {
    TestBed.configureTestingModule({
      imports: [
        TranslocoTestingModule.forRoot({
          langs: { en: translations },
          translocoConfig: { availableLangs: ['en'], defaultLang: 'en' },
          preloadLangs: true,
        }),
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'recipes', component: Recipes }]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
    const account = TestBed.inject(AccountSession);
    http
      .expectOne('/api/auth/session')
      .flush({ email: null, registrationOpen });
    await account.ready;
    fixture = TestBed.createComponent(SignIn);
    await fixture.whenStable();
    element = fixture.nativeElement;
  }

  async function submit() {
    for (const [selector, value] of [
      ['input[type=email]', 'a@example.com'],
      ['input[type=password]', 'any password'],
    ]) {
      const input = element.querySelector(selector) as HTMLInputElement;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    }
    (element.querySelector('form') as HTMLFormElement).dispatchEvent(
      new Event('submit', { cancelable: true }),
    );
    await fixture.whenStable();
  }

  async function requestTo(url: string) {
    let request: ReturnType<HttpTestingController['expectOne']> | undefined;
    await vi.waitFor(() => {
      const [match] = http.match(url);
      expect(match).toBeDefined();
      request = match;
    });
    return request as ReturnType<HttpTestingController['expectOne']>;
  }

  const alertText = async (expected: string) =>
    vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.querySelector('[role=alert]')?.textContent).toContain(
        expected,
      );
    });

  it('goes to the recipes once signed in', async () => {
    await setup();

    await submit();
    (await requestTo('/api/auth/login')).flush(null);
    (await requestTo('/api/auth/session')).flush({
      email: 'a@example.com',
      registrationOpen: false,
    });

    await vi.waitFor(() => expect(TestBed.inject(Router).url).toBe('/recipes'));
  });

  it('says the sign-in was refused on a 401', async () => {
    await setup();

    await submit();
    (await requestTo('/api/auth/login')).flush(null, {
      status: 401,
      statusText: 'Unauthorized',
    });

    await alertText('Refused');
  });

  it('can retry after a server failure, without changing a value', async () => {
    await setup();

    await submit();
    (await requestTo('/api/auth/login')).flush(null, {
      status: 500,
      statusText: 'Server Error',
    });
    await alertText('Unavailable');

    await submit();
    (await requestTo('/api/auth/login')).flush(null);
    (await requestTo('/api/auth/session')).flush({
      email: 'a@example.com',
      registrationOpen: false,
    });

    await vi.waitFor(() => expect(TestBed.inject(Router).url).toBe('/recipes'));
  });

  it('offers to create an account only while registration is open', async () => {
    await setup(true);

    expect(element.querySelector('a')?.getAttribute('href')).toBe(
      '/account/sign-up',
    );
  });

  it('offers no account creation while registration is closed', async () => {
    await setup(false);

    expect(element.querySelector('a')).toBeNull();
  });
});
