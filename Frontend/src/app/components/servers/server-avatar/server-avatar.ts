import { Component, Input } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-server-avatar',
  styleUrl: './server-avatar.scss',
  templateUrl: './server-avatar.html',
})
export class ServerAvatar {
  @Input() avatarUrl: string  = "";
  @Input() serverName: string  = "";

  trimmedServerName(): string {
      return this.serverName.substring(0, 7);
  }
}
