import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { isPlatformBrowser } from '@angular/common'; 

@Injectable({
  providedIn: 'root'
})
export class IconService {
  private platformId = inject(PLATFORM_ID);

  constructor(
    private matIconRegistry: MatIconRegistry,
    private domSanitizer: DomSanitizer
  ) {}

  registerIcons(): void {
    if (isPlatformBrowser(this.platformId)) { 
      console.log('IconService: Registering icons (Browser)...');
      this.matIconRegistry.addSvgIcon(
        'google-logo',
        this.domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/google-logo.svg')
      );
    } else {
      console.log('IconService: Skipping icon registration (Server)...');
    }
  }
}