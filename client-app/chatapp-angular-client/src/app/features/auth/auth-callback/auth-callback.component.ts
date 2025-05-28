import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service'; 

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  templateUrl: './auth-callback.component.html',
  styleUrl: './auth-callback.component.scss'
})
export class AuthCallbackComponent implements OnInit {
  isLoading = true;
  errorMessage: string | null = null;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  ngOnInit(): void {
    this.isLoading = true;
    this.route.queryParamMap.subscribe(params => {
      const token = params.get('token'); // Lấy token từ URL (API đã redirect về đây)
      // const userId = params.get('userId');
      // const displayName = params.get('displayName');

      if (token) {
        console.log('AuthCallbackComponent: Token received from API.');
        this.authService.handleAuthCallback(token)
          .then(() => {
            console.log('AuthCallbackComponent: Token processed, navigation should occur via AuthService.');
            this.isLoading = false;
          })
          .catch(error => {
            console.error('AuthCallbackComponent: Error processing token in AuthService', error);
            this.errorMessage = 'Login failed during token processing. Please try again.';
            this.isLoading = false;
            this.router.navigate(['/login'], { queryParams: { error: 'token_processing_failed' } });
          });
      } else {
        console.warn('AuthCallbackComponent: No token found in query parameters.');
        this.errorMessage = 'Authentication failed: No token received.';
        this.isLoading = false;
        this.router.navigate(['/login'], { queryParams: { error: 'no_token' } });
      }
    });
  }
}