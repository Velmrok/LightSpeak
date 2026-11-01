import { HttpClient } from '@angular/common/http';
import { inject, Injectable, Service } from '@angular/core';
import { map } from 'rxjs';
import { toSection } from '../models/section';
import { HomeDto, HomeView } from '../models/home';
import { ApiResponse } from '../models/api.response';
import { SectionNames } from '../models/section.names';

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
