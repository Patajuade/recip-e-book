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
import { SignUp } from './sign-up';

@Component({ template: '' })
class Recipes {}

const translations = {
  accounts: {
    email: 'Email',
    signUp: {
      heading: 'Create an account',
      password: 'Password',
      passwordHint: 'Hint',
      submit: 'Create',
      unavailable: 'Unavailable',
      closed: 'Closed',
      hasAccount: 'Has account',
      signIn: 'Sign in',
    },
  },
  forms: {
    errors: {
      required: 'Required',
      email: 'Bad email',
      minLength: 'At least {{minLength}}',
      maxLength: 'At most {{maxLength}}',
      server: '{{message}}',
    },
  },
};

describe('SignUp', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<SignUp>;
  let element: HTMLElement;

  async function setup(registrationOpen = true) {
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
    fixture = TestBed.createComponent(SignUp);
    await fixture.whenStable();
    element = fixture.nativeElement;
  }

  const type = (selector: string, value: string) => {
    const input = element.querySelector(selector) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  };

  async function submit(password = 'a long enough passphrase') {
    type('input[type=email]', 'a@example.com');
    type('input[type=password]', password);
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

  it('says so instead of the form when registration is closed', async () => {
    await setup(false);

    expect(element.querySelector('form')).toBeNull();
    expect(element.textContent).toContain('Closed');
  });

  it('refuses a short password without calling the API', async () => {
    await setup();

    await submit('fourteen chars');

    expect(element.textContent).toContain('At least 15');
    http.expectNone('/api/auth/register');
  });

  it('goes to the recipes once the account is created', async () => {
    await setup();

    await submit();
    (await requestTo('/api/auth/register')).flush(null);
    (await requestTo('/api/auth/session')).flush({
      email: 'a@example.com',
      registrationOpen: true,
    });

    await vi.waitFor(() => expect(TestBed.inject(Router).url).toBe('/recipes'));
  });

  it('shows the API errors under their field', async () => {
    await setup();

    await submit();
    (await requestTo('/api/auth/register')).flush(
      { errors: { Email: ['Already used.'] } },
      { status: 400, statusText: 'Bad Request' },
    );

    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('Already used.');
    });
    expect(element.querySelector('[role=alert]')).toBeNull();
  });

  it('shows the closed message when registration closed meanwhile', async () => {
    await setup();

    await submit();
    (await requestTo('/api/auth/register')).flush(null, {
      status: 403,
      statusText: 'Forbidden',
    });
    (await requestTo('/api/auth/session')).flush({
      email: null,
      registrationOpen: false,
    });

    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('Closed');
    });
  });
});
