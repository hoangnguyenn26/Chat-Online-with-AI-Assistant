import { Component, inject } from '@angular/core';
import { Router, RouterModule, NavigationStart } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'ChatApp Angular Client';

  private router = inject(Router);

  ngOnInit(): void {
    // Log router events for debugging
    this.router.events.pipe(
      filter(event => event instanceof NavigationStart)
    ).subscribe((event: NavigationStart) => {
      console.log('🚦 Router NavigationStart:', event.url);
    });
  }
}