import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { AuthService } from './app/core/services/auth.service'; // Import
import { IconService } from './app/core/services/icon.service';

bootstrapApplication(AppComponent, appConfig)
  .then(appRef => {
    const iconService = appRef.injector.get(IconService);
    iconService.registerIcons();
  })
  .catch((err) => console.error(err));