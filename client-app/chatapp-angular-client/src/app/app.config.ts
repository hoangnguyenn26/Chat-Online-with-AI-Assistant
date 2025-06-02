import { 
  ApplicationConfig, 
  PLATFORM_ID, 
  inject,
  importProvidersFrom,
  provideAppInitializer
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { MatIconModule } from '@angular/material/icon';

import { IconService } from './core/services/icon.service';
import { AuthService } from './core/services/auth.service';
import { authInterceptorFn } from './core/http-interceptors/auth.interceptor.fn';
import { appRoutes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes, withComponentInputBinding()),
    provideAnimations(),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptorFn])
    ),
    importProvidersFrom(MatIconModule),
    IconService,
    AuthService,

    provideAppInitializer(() => {
      const iconService = inject(IconService);
      const platformId = inject(PLATFORM_ID);
      
      if (isPlatformBrowser(platformId)) {
        iconService.registerIcons();
      }
    }),
    
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      const platformId = inject(PLATFORM_ID);
      
      if (isPlatformBrowser(platformId)) {
        return authService.init(); 
      }
      return Promise.resolve();
    })
  ]
};