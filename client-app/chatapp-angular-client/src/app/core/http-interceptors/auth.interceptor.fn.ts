import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authInterceptorFn: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const authService = inject(AuthService); 
  const authToken = authService.getCurrentToken();
  const router = inject(Router);

  let authReq = req;
  if (authToken && !req.url.includes('/auth/login') && !req.url.includes('/auth/register')) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${authToken}`,
      },
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        console.warn('AuthInterceptorFn: Received 401 Unauthorized. Logging out.');
        authService.logout(); // AuthService sẽ tự điều hướng
      }
      return throwError(() => error);
    })
  );
};