import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AccountSession } from '@recip-e-book/web-accounts-data-access';
import { getTranslocoModule } from '../../testing/transloco-testing';
import { Landing } from './landing';

describe('Landing', () => {
  let fixture: ComponentFixture<Landing>;

  async function setup(registrationOpen: boolean) {
    await TestBed.configureTestingModule({
      imports: [Landing, getTranslocoModule()],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(Landing);
    TestBed.inject(HttpTestingController)
      .expectOne('/api/auth/session')
      .flush({ email: null, registrationOpen });
    await TestBed.inject(AccountSession).ready;
    await fixture.whenStable();
  }

  const links = () =>
    [...(fixture.nativeElement as HTMLElement).querySelectorAll('a')].map(
      (link) => link.getAttribute('href'),
    );

  it('leads to signing in, and up while registration is open', async () => {
    await setup(true);

    expect(links()).toEqual(['/account/sign-in', '/account/sign-up']);
  });

  it('only leads to signing in while registration is closed', async () => {
    await setup(false);

    expect(links()).toEqual(['/account/sign-in']);
  });
});
