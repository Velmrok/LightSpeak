import { Component } from '@angular/core';
import { ChannelList } from '../../components/servers/channel-list/channel-list';

@Component({
  imports: [ChannelList],
  selector: 'app-server-page',
  styleUrl: './server-page.scss',
  templateUrl: './server-page.html',
})
export class ServerPage {}
