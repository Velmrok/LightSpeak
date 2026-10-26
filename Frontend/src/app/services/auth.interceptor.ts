import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { AuthService } from "./auth.service";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const apiReq = req.clone({ withCredentials: true });

  return next(apiReq).pipe(
    catchError((err: HttpErrorResponse) => {
      const isAuthCall = req.url.endsWith('/me') || req.url.endsWith('/login');

      if (err.status === 401 && !isAuthCall) {
        auth.clear();
        router.navigate(['/api/login']);
      }
      return throwError(() => err);
    }),
  );
};