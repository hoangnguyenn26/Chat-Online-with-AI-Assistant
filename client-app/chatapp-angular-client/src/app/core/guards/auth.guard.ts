import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isLoggedIn$.pipe(
    take(1), 
    map(isLoggedIn => {
      if (isLoggedIn) {
        return true; 
      } else {
        console.warn('AuthGuard: User not logged in. Redirecting to /login.');
        return router.createUrlTree(['/login'], { queryParams: { returnUrl: router.url } });
      }
    })
  );
};