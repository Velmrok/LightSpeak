import { Routes } from '@angular/router';
import { HomePage } from './pages/home-page/home-page';
import { ServerPage } from './pages/server-page/server-page';

export const routes: Routes = [
{ path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: HomePage },
  { path: 'servers/:serverId', component: ServerPage },
];
