import { ApplicationConfig, importProvidersFrom, inject, provideAppInitializer } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { appRoutes } from './app.routes';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';

// IconService and AuthService
import { IconService } from './core/services/icon.service';
import { AuthService } from './core/services/auth.service';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';

// HTTP Interceptor
import { authInterceptorFn } from './core/http-interceptors/auth.interceptor.fn';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes, withComponentInputBinding()),
    provideAnimations(),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptorFn])
    ),
    IconService,
    AuthService,
    provideAppInitializer(() => {
      const iconService = inject(IconService);
      return iconService.registerIcons();
    }),
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return authService.init();
    }),
  ]
};