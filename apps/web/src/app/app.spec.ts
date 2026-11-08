import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideServiceWorker } from '@angular/service-worker';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { getTranslocoModule } from '../testing/transloco-testing';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App, getTranslocoModule()],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideServiceWorker('ngsw-worker.js', { enabled: false }),
      ],
    }).compileComponents();
  });

  it('should render the app name in the header', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('header a')?.textContent).toContain(
      'Recip-e-book',
    );
  });

  async function headerFor(email: string | null | 'offline') {
    const fixture = TestBed.createComponent(App);
    const request = TestBed.inject(HttpTestingController).expectOne(
      '/api/auth/session',
    );
    if (email === 'offline') {
      request.error(new ProgressEvent('error'), { status: 0 });
    } else {
      request.flush({ email, registrationOpen: false });
    }
    await TestBed.inject(AccountSession).ready;
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement).querySelector(
      'nav[aria-label="Main"]',
    );
  }

  it('hides the recipes and lists links from a visitor', async () => {
    expect(await headerFor(null)).toBeNull();
  });

  it('shows them once signed in', async () => {
    expect(await headerFor('a@example.com')).not.toBeNull();
  });

  it('shows them offline, when the session is unknown', async () => {
    expect(await headerFor('offline')).not.toBeNull();
  });
});
