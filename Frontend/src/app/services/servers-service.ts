import { inject, Service, signal } from '@angular/core';
import { ServerDetails, ServerSummary } from '../models/server';
import { CreateServerRequest, CreateServerResponse } from '../models/api/create.server';
import { tap } from 'rxjs/internal/operators/tap';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../models/api.response';
import { map, of } from 'rxjs';

@Service()
export class ServersService {
    http = inject(HttpClient);
    serversSummary = signal<ServerSummary[]>([]);
    serversDetails = signal<Map<string, ServerDetails>>(new Map());

    add(server: ServerSummary) {
        this.serversSummary.update(list => [server,...list]);
    }
    create(request: CreateServerRequest) {
        return this.http.post<ApiResponse<CreateServerResponse>>('/api/servers', request).pipe(
            tap((res) => this.add({ id: res.data.serverId, name: request.name, avatarUrl: '' }))
        );
    }
    getServerDetails(serverId: string){
        const cached = this.serversDetails().get(serverId);
        if (cached) {
            return of(cached);
        }

        return this.http
            .get<ApiResponse<ServerDetails>>(`/api/servers/${serverId}`)
            .pipe(
                map((res) => res.data),
                tap((details) =>
                    this.serversDetails.update((m) => new Map(m).set(serverId, details))
                )
            );
    }
}
