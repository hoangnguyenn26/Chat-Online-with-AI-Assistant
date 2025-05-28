import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterModule, NavigationStart } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'ChatApp Angular Client';
  
  private authService = inject(AuthService);
  private router = inject(Router);

  async ngOnInit(): Promise<void> {
    console.log('🔧 AppComponent initialized, initializing AuthService');
    await this.authService.init();

    // Log router events for debugging
    this.router.events.pipe(
      filter(event => event instanceof NavigationStart)
    ).subscribe((event: NavigationStart) => {
      console.log('🚦 Router NavigationStart:', event.url);
    });
  }
}