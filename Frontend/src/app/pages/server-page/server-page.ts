import { Component, inject } from '@angular/core';
import { ChannelList } from '../../components/servers/channel-list/channel-list';
import { ServersService } from '../../services/servers-service';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs/internal/operators/map';
import { rxResource,  toSignal } from '@angular/core/rxjs-interop';

@Component({
  imports: [ChannelList],
  selector: 'app-server-page',
  styleUrl: './server-page.scss',
  templateUrl: './server-page.html',
})
export class ServerPage {
  private activatedRoute = inject(ActivatedRoute);
  serverId = toSignal(
    this.activatedRoute.paramMap.pipe(map((p) => p.get('serverId')!)),
    { requireSync: true }
  );
  serversService = inject(ServersService);
  details = rxResource({
    params: () => this.serverId(),
    stream: ({ params: id }) => this.serversService.getServerDetails(id),
  });
}
