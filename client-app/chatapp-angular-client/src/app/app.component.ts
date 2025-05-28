import { Component, OnInit, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'ChatApp Angular Client';
  
  // Inject AuthService để trigger khởi tạo
  private authService = inject(AuthService);

  ngOnInit(): void {
    console.log('🔧 AppComponent initialized, AuthService injected');
  }
}