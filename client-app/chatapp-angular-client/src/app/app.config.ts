// src/app/app.config.ts
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { appRoutes } from './app.routes';
import { provideHttpClient, withInterceptorsFromDi, withFetch } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';

// IconService
import { IconService } from './core/services/icon.service';
import { APP_INITIALIZER } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';


export function initializeIconsFactory(iconService: IconService) {
  return () => iconService.registerIcons();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes, withComponentInputBinding()),
    provideAnimations(),
    provideHttpClient(withFetch(), withInterceptorsFromDi()),
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