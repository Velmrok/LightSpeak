import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ServersService } from '../../../services/servers-service';
import { ChannelService } from '../../../services/channel-service';

@Component({
  imports: [],
  selector: 'app-channel-create-form',
  styleUrl: './channel-create-form.scss',
  templateUrl: './channel-create-form.html',
})
export class ChannelCreateForm {
  private _dialogRef: any = inject(MatDialogRef<ChannelCreateForm>)
  private _formBuilder = inject(FormBuilder);
  channelService = inject(ChannelService);
  channelName: string = '';
  form = this._formBuilder.group({
    name: ['', Validators.required],
  });
  loading = signal(false);
  error = signal<string | null>(null);

  submit() {
    if (this.form.invalid) return;

    this.loading.set(true);
    this.error.set(null);
    const name = this.form.getRawValue().name ?? '';
    this.channelService.create({ name }).subscribe({
      next: (res) => {
        this._dialogRef.close(true);
        this.error.set(null);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Nie udało się utworzyć kanału');
        this.loading.set(false);
      },
    });
  }
  closeModal() {
    this._dialogRef.close();
  }
}
