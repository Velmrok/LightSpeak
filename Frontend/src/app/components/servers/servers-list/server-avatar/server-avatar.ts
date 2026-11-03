import { Component, Input } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ServerSummary } from '../../../../models/server';


@Component({
  imports: [RouterLink, RouterLinkActive],
  selector: 'app-server-avatar',
  styleUrl: './server-avatar.scss',
  templateUrl: './server-avatar.html',
})
export class ServerAvatar {
  @Input() serverSummary : ServerSummary = { id: '', name: '', avatarUrl: '' };
  avatarUrl(): string {
      return this.serverSummary.avatarUrl;
  }

  trimmedServerName(): string {
      return this.serverSummary.name.substring(0, 7);
  }
}
