import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, of, retry, tap, throwError, timer } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
   private http = inject(HttpClient);
  private router = inject(Router);

  private _user = signal<User | null>(null);
  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);

  loadUser() {
  return this.http.get<User>('/api/profile/me').pipe(
    retry({
      count: 2,
      delay: (err) => err.status === 404 ? timer(500) : throwError(() => err),
    }),
    tap(user => this._user.set(user)),
    catchError(() => {
      this._user.set(null);
      return of(null);
    }),
  );
}

  login() {

    window.location.href = '/api/login';
  }

 

  clear() {
    this._user.set(null);
  }
}
