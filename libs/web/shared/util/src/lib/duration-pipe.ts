import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'duration',
})
export class DurationPipe implements PipeTransform {
  transform(totalMinutes: number | null | undefined, locale: string): string {
    if (!totalMinutes) {
      return '';
    }
    return new Intl.DurationFormat(locale, { style: 'short' }).format({
      hours: Math.floor(totalMinutes / 60),
      minutes: totalMinutes % 60,
    });
  }
}
