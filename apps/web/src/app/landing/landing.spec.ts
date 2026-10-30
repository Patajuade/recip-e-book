import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { getTranslocoModule } from '../../testing/transloco-testing';
import { Landing } from './landing';

describe('Landing', () => {
  let fixture: ComponentFixture<Landing>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Landing, getTranslocoModule()],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Landing);
    await fixture.whenStable();
  });

  it('should lead to the recipes and the shopping lists', () => {
    const links = [
      ...(fixture.nativeElement as HTMLElement).querySelectorAll('a'),
    ].map((link) => link.getAttribute('href'));

    expect(links).toEqual(['/recipes', '/shopping']);
  });
});
