import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { IconService } from './app/core/services/icon.service';
import { APP_INITIALIZER } from '@angular/core';

// Hàm factory cho APP_INITIALIZER
export function initializeIcons(iconService: IconService) {
  return () => iconService.registerIcons();
}

bootstrapApplication(AppComponent, appConfig)
 .then(appRef => { // Lấy instance IconService sau khi app bootstrap và gọi register
     const iconService = appRef.injector.get(IconService);
     iconService.registerIcons();
 })
 .catch((err) => console.error(err));