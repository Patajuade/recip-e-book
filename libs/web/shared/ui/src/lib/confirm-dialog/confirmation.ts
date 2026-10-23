import { Dialog } from '@angular/cdk/dialog';
import { inject, Service } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ConfirmDialog, ConfirmDialogData } from './confirm-dialog';

@Service()
export class Confirmation {
  private readonly dialog = inject(Dialog);

  /** Resolves to true only when the user confirms (Escape and backdrop click cancel). */
  async ask(data: ConfirmDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open<boolean>(ConfirmDialog, {
      data,
      role: 'alertdialog',
      ariaLabelledBy: 'confirm-dialog-title',
      ariaDescribedBy: 'confirm-dialog-message',
    });
    return (await firstValueFrom(dialogRef.closed)) === true;
  }
}
