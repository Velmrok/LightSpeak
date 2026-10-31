import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Service } from '@angular/core';
import { map } from 'rxjs';

@Service()
export class HomeService {
  http = inject(HttpClient);

  getHome() {
    return this.http.get<ApiResponse<HomeDto>>('/api/home').pipe(
      map((res): HomeView => ({
        servers: toSection(res.data?.servers, res.errors, SectionNames.SERVERS),
        friends: toSection(res.data?.friends, res.errors, SectionNames.PROFILE),
      })),
    );
  }
}
