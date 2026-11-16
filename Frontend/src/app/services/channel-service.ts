import { Service, signal } from '@angular/core';
import { ChannelSummary } from '../models/server';

@Service()
export class ChannelService {
    private channelsSummary= signal<Map<string, ChannelSummary[]>>(new Map());
    createChannel(name: string){

    }
}
