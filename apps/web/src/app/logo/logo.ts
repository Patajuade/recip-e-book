import { Component } from '@angular/core';

/** The « Marmite » logo, sized by the class given to `<app-logo>`. */
@Component({
  selector: 'app-logo',
  templateUrl: './logo.html',
  styleUrl: './logo.css',
  host: { class: 'block' },
})
export class Logo {}
