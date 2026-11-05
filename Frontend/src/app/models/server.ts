
export interface ServerSummary {
  id: string;
  name: string;
  avatarUrl: string;
}
export interface ServerDetails extends ServerSummary {
  channels: ChannelSummary[];
}

export interface ChannelSummary {
  id: string;
  name: string;
  type: ChannelType;
}

export enum ChannelType {
  Text = 'text',
  Voice = 'voice'
}