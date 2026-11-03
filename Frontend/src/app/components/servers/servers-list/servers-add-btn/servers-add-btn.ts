import { Component, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ServersCreateForm } from '../../servers-create-form/servers-create-form';


@Component({
  imports: [],
  selector: 'app-servers-add-btn',
  styleUrl: './servers-add-btn.scss',
  templateUrl: './servers-add-btn.html',
})
export class ServersAddBtn {
  private _dialog: any = inject(MatDialog);

  openCreateForm() {
    this._dialog.open(ServersCreateForm);
  }
}
