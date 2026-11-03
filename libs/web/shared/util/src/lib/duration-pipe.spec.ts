import { DurationPipe } from './duration-pipe';

describe('DurationPipe', () => {
  const pipe = new DurationPipe();

  it('formats hours and minutes in the given language', () => {
    expect(pipe.transform(75, 'en')).toBe(
      new Intl.DurationFormat('en', { style: 'short' }).format({
        hours: 1,
        minutes: 15,
      }),
    );
    expect(pipe.transform(75, 'fr')).toBe(
      new Intl.DurationFormat('fr', { style: 'short' }).format({
        hours: 1,
        minutes: 15,
      }),
    );
  });

  it('leaves out the zero units', () => {
    expect(pipe.transform(45, 'en')).not.toMatch(/hr/);
    expect(pipe.transform(120, 'en')).not.toMatch(/min/);
  });

  it('is empty without a duration', () => {
    expect(pipe.transform(null, 'en')).toBe('');
    expect(pipe.transform(undefined, 'en')).toBe('');
    expect(pipe.transform(0, 'en')).toBe('');
  });
});
