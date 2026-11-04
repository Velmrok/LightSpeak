import { inject, Service, signal } from '@angular/core';
import { ServerSummary } from '../models/server';
import { CreateServerRequest, CreateServerResponse } from '../models/api/create.server';
import { tap } from 'rxjs/internal/operators/tap';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../models/api.response';

@Service()
export class ServersService {
    http = inject(HttpClient);
    serversSummary = signal<ServerSummary[]>([]);

    add(server: ServerSummary) {
        this.serversSummary.update(list => [server,...list]);
    }
    create(request: CreateServerRequest) {
        return this.http.post<ApiResponse<CreateServerResponse>>('/api/servers', request).pipe(
            tap((res) => this.add({ id: res.data.serverId, name: request.name, avatarUrl: '' }))
        );
    }
}
