import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { getTranslocoModule } from '../../testing/transloco-testing';
import { AccountMenu } from './account-menu';

describe('AccountMenu', () => {
  let http: HttpTestingController;
  let fixture: ComponentFixture<AccountMenu>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountMenu, getTranslocoModule()],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(AccountMenu);
    await fixture.whenStable();
  });

  const text = () =>
    (fixture.nativeElement as HTMLElement).textContent?.trim() ?? '';

  async function answer(session: {
    email: string | null;
    registrationOpen: boolean;
  }) {
    http.expectOne('/api/auth/session').flush(session);
    await TestBed.inject(AccountSession).ready;
    await fixture.whenStable();
  }

  it('shows nothing while the session is unknown', () => {
    expect(text()).toBe('');
    http.expectOne('/api/auth/session').flush({
      email: null,
      registrationOpen: false,
    });
  });

  it('offers to sign in to a visitor', async () => {
    await answer({ email: null, registrationOpen: false });

    const link = (fixture.nativeElement as HTMLElement).querySelector('a');
    expect(link?.getAttribute('href')).toBe('/account/sign-in');
  });

  it('shows the email and signing out to someone signed in', async () => {
    await answer({ email: 'a@example.com', registrationOpen: false });

    expect(text()).toContain('a@example.com');
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('button'),
    ).not.toBeNull();
  });
});
