import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { Section } from '../../../models/section';
import { ServerSummary } from '../../../models/server';

@Component({
  selector: 'app-servers-list',
  imports: [],
  templateUrl: './servers-list.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './servers-list.scss',
})
export class ServersList {
  @Input() serversSection: Section<ServerSummary[]> = { status: 'ok', data: [] };
  
}
