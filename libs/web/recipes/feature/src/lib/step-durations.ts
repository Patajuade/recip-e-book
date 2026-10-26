export type StepSegment =
  | { kind: 'text'; text: string }
  | { kind: 'duration'; text: string; seconds: number };

// French and English: « 1 h 30 », « 1h30 », « 2 heures », « 10 min », « 45 secondes »,
// « 1 hour », « 10 minutes », « 30 sec ». The unit must not be followed by a letter, so
// « 4 hamburgers » or « 2 sachets » aren't durations. In « 10 à 15 min » only « 15 min » is.
const DURATION =
  /(\d+)\s*(?:heures?|hours?|hrs?|h)(?:\s*(\d{1,2})(?:\s*(?:minutes?|min|mn))?)?(?!\p{L})|(\d+)\s*(?:minutes?|min|mn)(?!\p{L})|(\d+)\s*(?:secondes?|seconds?|sec|s)(?!\p{L})/giu;

/** Splits a step into text and the durations it mentions, which become timer buttons. */
export function splitDurations(step: string): StepSegment[] {
  const segments: StepSegment[] = [];
  let last = 0;
  for (const match of step.matchAll(DURATION)) {
    const [text, hours, hourMinutes, minutes, seconds] = match;
    const total = hours
      ? Number(hours) * 3600 + Number(hourMinutes ?? 0) * 60
      : minutes
        ? Number(minutes) * 60
        : Number(seconds);
    if (total === 0) {
      continue;
    }
    if (match.index > last) {
      segments.push({ kind: 'text', text: step.slice(last, match.index) });
    }
    segments.push({ kind: 'duration', text, seconds: total });
    last = match.index + text.length;
  }
  if (last < step.length) {
    segments.push({ kind: 'text', text: step.slice(last) });
  }
  return segments;
}
