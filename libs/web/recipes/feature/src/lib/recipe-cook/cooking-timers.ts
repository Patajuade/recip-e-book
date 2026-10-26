import { computed, DestroyRef, inject, Service, signal } from '@angular/core';

export interface CookingTimer {
  id: number;
  label: string;
  /** Running: when it rings (epoch ms). */
  endsAt: number | null;
  /** Paused: the milliseconds left. */
  pausedLeft: number | null;
  done: boolean;
}

/**
 * The timers started from the steps. Provided by the cooking page, so they stop when it
 * closes. Remaining times are computed from the end time, not counted down, so they stay
 * right even when the browser slows the interval down.
 */
@Service({ autoProvided: false })
export class CookingTimers {
  readonly timers = signal<CookingTimer[]>([]);
  /** Labels of the timers that just rang, for the alert region. */
  readonly rang = computed(() =>
    this.timers()
      .filter((timer) => timer.done)
      .map((timer) => timer.label),
  );

  private readonly now = signal(Date.now());
  private nextId = 0;
  private audio: AudioContext | null = null;

  constructor() {
    const interval = setInterval(() => this.tick(), 250);
    inject(DestroyRef).onDestroy(() => {
      clearInterval(interval);
      void this.audio?.close();
    });
  }

  /** Called from a click: the audio context is created during a user action (autoplay rules). */
  start(label: string, seconds: number): void {
    this.audio ??= new AudioContext();
    void this.audio.resume();
    const timer: CookingTimer = {
      id: this.nextId++,
      label,
      endsAt: Date.now() + seconds * 1000,
      pausedLeft: null,
      done: false,
    };
    this.timers.update((timers) => [...timers, timer]);
  }

  leftMs(timer: CookingTimer): number {
    if (timer.pausedLeft !== null) {
      return timer.pausedLeft;
    }
    return Math.max(0, (timer.endsAt ?? 0) - this.now());
  }

  pause(id: number): void {
    this.patch(id, (timer) => ({
      ...timer,
      pausedLeft: this.leftMs(timer),
      endsAt: null,
    }));
  }

  resume(id: number): void {
    this.patch(id, (timer) => ({
      ...timer,
      endsAt: Date.now() + (timer.pausedLeft ?? 0),
      pausedLeft: null,
    }));
  }

  remove(id: number): void {
    this.timers.update((timers) => timers.filter((timer) => timer.id !== id));
  }

  private tick(): void {
    this.now.set(Date.now());
    const finished = this.timers().filter(
      (timer) =>
        !timer.done && timer.endsAt !== null && timer.endsAt <= this.now(),
    );
    if (!finished.length) {
      return;
    }
    const ids = new Set(finished.map((timer) => timer.id));
    this.timers.update((timers) =>
      timers.map((timer) =>
        ids.has(timer.id) ? { ...timer, done: true, endsAt: null } : timer,
      ),
    );
    this.ring();
  }

  private patch(
    id: number,
    change: (timer: CookingTimer) => CookingTimer,
  ): void {
    this.timers.update((timers) =>
      timers.map((timer) => (timer.id === id ? change(timer) : timer)),
    );
  }

  /** Three short beeps, and a vibration where the device supports it. */
  private ring(): void {
    if (this.audio) {
      const start = this.audio.currentTime;
      for (let i = 0; i < 3; i++) {
        const oscillator = new OscillatorNode(this.audio, { frequency: 880 });
        const gain = new GainNode(this.audio, { gain: 0.2 });
        oscillator.connect(gain).connect(this.audio.destination);
        oscillator.start(start + i * 0.4);
        oscillator.stop(start + i * 0.4 + 0.2);
      }
    }
    if ('vibrate' in navigator) {
      navigator.vibrate([200, 100, 200, 100, 200]);
    }
  }
}
