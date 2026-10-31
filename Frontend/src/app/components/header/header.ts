import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { AuthService } from '../../services/auth-service';
import { HomeService } from '../../services/home-service';
import { toSignal } from '@angular/core/rxjs-interop';
import { ServersList } from '../servers/servers-list/servers-list';

@Component({
  selector: 'app-header',
  imports: [ServersList],
  templateUrl: './header.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './header.scss',
})
export class Header {
  auth = inject(AuthService);
  homeService = inject(HomeService);
  home =  toSignal(this.homeService.getHome());
}
