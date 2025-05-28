import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state): Observable<boolean | UrlTree> => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isLoggedIn$.pipe(
    take(1),
    map(isLoggedIn => {
      if (isLoggedIn) {
        console.log('✅ AuthGuard: User is logged in, allowing access.');
        return true;
      } else {
        console.warn('⚠️ AuthGuard: User not logged in, redirecting to /login.');
        const returnUrl = state.url !== '/login' ? state.url : '/chat';
        return router.createUrlTree(['/login'], { queryParams: { returnUrl } });
      }
    })
  );
};