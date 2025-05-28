// src/app/app.config.ts
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { appRoutes } from './app.routes';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';

// IconService
import { IconService } from './core/services/icon.service';
import { APP_INITIALIZER } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';

// HTTP Interceptor
import { authInterceptorFn } from './core/http-interceptors/auth.interceptor.fn';

export function initializeIconsFactory(iconService: IconService) {
  return () => iconService.registerIcons();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes, withComponentInputBinding()),
    provideAnimations(),
    provideHttpClient(
      withFetch(), 
      withInterceptors([authInterceptorFn])
    ), 
    importProvidersFrom(MatIconRegistry),
    IconService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeIconsFactory,
      deps: [IconService],
      multi: true
    }
  ]
};