import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TranslocoService, TranslocoTestingModule } from '@jsverse/transloco';
import { acceptLanguageInterceptor } from './accept-language-interceptor';

describe('acceptLanguageInterceptor', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TranslocoTestingModule.forRoot({
          langs: { en: {}, fr: {} },
          translocoConfig: { availableLangs: ['en', 'fr'], defaultLang: 'en' },
        }),
      ],
      providers: [
        provideHttpClient(withInterceptors([acceptLanguageInterceptor])),
        provideHttpClientTesting(),
      ],
    });
  });

  it('sends the active language to the API', () => {
    TestBed.inject(TranslocoService).setActiveLang('fr');

    TestBed.inject(HttpClient).get('/api/recipes').subscribe();

    const request = TestBed.inject(HttpTestingController).expectOne(
      '/api/recipes',
    );
    expect(request.request.headers.get('Accept-Language')).toBe('fr');
    request.flush([]);
  });
});
