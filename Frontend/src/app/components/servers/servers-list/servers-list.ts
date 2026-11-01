import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { Section } from '../../../models/section';
import { ServerSummary } from '../../../models/server';
import { ServerAvatar } from '../server-avatar/server-avatar';

@Component({
  selector: 'app-servers-list',
  imports: [ServerAvatar],
  templateUrl: './servers-list.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './servers-list.scss',
})
export class ServersList {
  @Input() serversSection: Section<ServerSummary[]> = { status: 'ok', data: [] };
  
}
