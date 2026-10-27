import { DOCUMENT, inject, Service } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { SwUpdate } from '@angular/service-worker';
import { filter, map } from 'rxjs';

/**
 * What the service worker says about versions (Angular "Communicating with the service
 * worker" guide). Its observables never emit when it is disabled (dev server).
 */
@Service()
export class AppUpdates {
  private readonly swUpdate = inject(SwUpdate);
  private readonly document = inject(DOCUMENT);

  /** A new version is downloaded: it runs after a reload. */
  readonly ready = toSignal(
    this.swUpdate.versionUpdates.pipe(
      filter((event) => event.type === 'VERSION_READY'),
      map(() => true),
    ),
    { initialValue: false },
  );

  /** Files of the running version are gone (e.g. evicted from the cache): only a reload fixes it. */
  readonly broken = toSignal(
    this.swUpdate.unrecoverable.pipe(map(() => true)),
    {
      initialValue: false,
    },
  );

  reload(): void {
    this.document.location.reload();
  }
}
