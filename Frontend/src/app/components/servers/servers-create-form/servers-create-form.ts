import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ServersService } from '../../../services/servers-service';

@Component({
  selector: 'app-servers-create-form',
  imports: [FormsModule, ReactiveFormsModule, MatDialogModule],
  templateUrl: './servers-create-form.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './servers-create-form.scss',
})
export class ServersCreateForm {
  private _dialogRef: any = inject(MatDialogRef<ServersCreateForm>)
  private _formBuilder = inject(FormBuilder);
  serversService = inject(ServersService);
  serverName: string = '';
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
    this.serversService.create({ name }).subscribe({
      next: (res) => {
        this._dialogRef.close(true);
        this.error.set(null);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Nie udało się utworzyć serwera');
        this.loading.set(false);
      },
    });
  }
  closeModal() {
    this._dialogRef.close();
  }
}
