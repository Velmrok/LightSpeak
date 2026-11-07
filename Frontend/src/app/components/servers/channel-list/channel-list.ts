import { Component, inject, Input } from '@angular/core';
import { ChannelSummary } from '../../../models/server';

@Component({
  imports: [],
  selector: 'app-channel-list',
  styleUrl: './channel-list.scss',
  templateUrl: './channel-list.html',
})
export class ChannelList {
  @Input() channelList: ChannelSummary[] = [];
}
