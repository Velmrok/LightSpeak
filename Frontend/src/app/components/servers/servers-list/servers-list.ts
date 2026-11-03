import { Component, ChangeDetectionStrategy, Input, inject } from '@angular/core';
import { Section } from '../../../models/section';
import { ServerSummary } from '../../../models/server';
import { ServerAvatar } from './server-avatar/server-avatar';
import { ServersAddBtn } from './servers-add-btn/servers-add-btn';
import { ServersService } from '../../../services/servers-service';

@Component({
  selector: 'app-servers-list',
  imports: [ServerAvatar,ServersAddBtn],
  templateUrl: './servers-list.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './servers-list.scss',
})
export class ServersList {
  private _serversService: ServersService = inject(ServersService);
  private _serversSection: Section<ServerSummary[]> = { status: 'ok', data: [] };
  @Input()
  set serversSection(value: Section<ServerSummary[]>) {
    if (value.status === 'ok') {
      this._serversService.serversSummary.set(value.data);
    } else {
      this._serversService.serversSummary.set([]);
    }
  }
  get serversSection(): Section<ServerSummary[]> {
    return this._serversSection;
  }
  get serversList(): ServerSummary[] {
    return this._serversService.serversSummary();
  }



  
  

}
