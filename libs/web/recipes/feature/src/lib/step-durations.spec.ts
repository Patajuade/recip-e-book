import { splitDurations } from './step-durations';

describe('splitDurations', () => {
  const timers = (step: string) =>
    splitDurations(step)
      .filter((segment) => segment.kind === 'duration')
      .map((segment) => [segment.text, segment.seconds]);

  it('finds French and English durations', () => {
    expect(timers('Cuire 10 min puis laisser reposer 45 secondes.')).toEqual([
      ['10 min', 600],
      ['45 secondes', 45],
    ]);
    expect(timers('Bake for 1 hour, then rest 30 sec.')).toEqual([
      ['1 hour', 3600],
      ['30 sec', 30],
    ]);
  });

  it('reads hours with minutes', () => {
    expect(timers('Mijoter 1 h 30.')).toEqual([['1 h 30', 5400]]);
    expect(timers('Mijoter 1h30.')).toEqual([['1h30', 5400]]);
    expect(timers('Laisser lever 2 heures.')).toEqual([['2 heures', 7200]]);
  });

  it('keeps only the upper bound of a range', () => {
    expect(timers('Cuire 10 à 15 min.')).toEqual([['15 min', 900]]);
  });

  it("doesn't take a word starting with a unit for a duration", () => {
    expect(timers('Ajouter 4 hamburgers et 2 sachets.')).toEqual([]);
  });

  it('ignores zero durations', () => {
    expect(timers('0 min')).toEqual([]);
  });

  it('keeps all the text around the durations', () => {
    const step = 'Cuire 10 min, puis servir.';

    expect(
      splitDurations(step)
        .map((segment) => segment.text)
        .join(''),
    ).toBe(step);
    expect(splitDurations(step)[0]).toEqual({ kind: 'text', text: 'Cuire ' });
  });
});
