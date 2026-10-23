import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, inject } from '@angular/core';
import { TranslocoDirective } from '@jsverse/transloco';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel: string;
}

/** Content of the dialog opened by `Confirmation.ask()`. */
@Component({
  selector: 'lib-confirm-dialog',
  imports: [TranslocoDirective],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
  host: {
    class: 'flex max-w-md flex-col gap-4 rounded-lg bg-white p-6 shadow-xl',
  },
})
export class ConfirmDialog {
  protected readonly data = inject<ConfirmDialogData>(DIALOG_DATA);
  protected readonly dialogRef = inject<DialogRef<boolean>>(DialogRef);
}
