import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http'; 
import { environment } from '../../../environments/environment'; 
import { UserDto, LoginResponseDto } from '../models/auth.dtos';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_BASE_URL = environment.apiBaseUrl; // Ví dụ: http://localhost:7001/api
  private router = inject(Router);
  private http = inject(HttpClient);

  private _authToken: string | null = null;

  // BehaviorSubject để quản lý trạng thái đăng nhập và thông tin user
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$: Observable<boolean> = this.isLoggedInSubject.asObservable();

  private currentUserSubject = new BehaviorSubject<UserDto | null>(this.getUserFromStorage());
  public currentUser$: Observable<UserDto | null> = this.currentUserSubject.asObservable();

  constructor() {
    this.checkInitialLoginState();
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('app_auth_token');
  }

  private getUserFromStorage(): UserDto | null {
    const userJson = localStorage.getItem('app_current_user');
    if (userJson) {
      try {
        return JSON.parse(userJson) as UserDto;
      } catch (e) {
        console.error('Error parsing user from localStorage', e);
        localStorage.removeItem('app_current_user'); // Xóa nếu lỗi
        return null;
      }
    }
    return null;
  }

  private async checkInitialLoginState(): Promise<void> {
    const token = localStorage.getItem('app_auth_token');
    if (token) {
      this._authToken = token;
      console.log('AuthService: Token found in storage. Verifying with /auth/me...');
      try {
        const userProfile = await this.http.get<UserDto>(`${this.API_BASE_URL}/auth/me`).toPromise();
        if (userProfile) {
          localStorage.setItem('app_current_user', JSON.stringify(userProfile));
          this.currentUserSubject.next(userProfile);
          this.isLoggedInSubject.next(true);
          console.log('AuthService: Session restored for user:', userProfile.userName);
        } else {
          console.warn('AuthService: Token verification failed or no user profile from /auth/me.');
          await this.logout();
        }
      } catch (error: any) {
        console.error('AuthService: Error verifying token with /auth/me.', error);
        if (error?.status === 401 || error?.name === 'HttpErrorResponse') {
             await this.logout();
        } else {
          console.error('AuthService: Unexpected error during token verification:', error);
        }
      }
    } else {
      this.isLoggedInSubject.next(false);
      this.currentUserSubject.next(null);
      console.log('AuthService: No token found in storage.');
    }
  }

  public getCurrentToken(): string | null {
      return this._authToken || localStorage.getItem('app_auth_token');
  }


  loginWithGoogle(): void {
    const currentAppUrl = window.location.origin; // Ví dụ: http://localhost:4200
    const apiLoginGoogleUrl = `${this.API_BASE_URL}/auth/login-google`;

    console.log(`Redirecting to Google login via API: ${apiLoginGoogleUrl}`);
    window.location.href = apiLoginGoogleUrl; // Thực hiện redirect
  }

  // Sẽ được gọi từ AuthCallbackComponent
  async handleAuthCallback(token: string): Promise<void> {
    console.log('AuthService: Handling auth callback with token:', token ? 'Token Received' : 'No Token');
    if (token) {
      localStorage.setItem('app_auth_token', token);
      this._authToken = token; // Cập nhật token nội bộ

      // **GỌI API /auth/me ĐỂ LẤY THÔNG TIN USER**
      try {
        console.log('AuthService: Calling /auth/me to fetch user profile...');
        // HttpClient đã được cấu hình để AuthInterceptor (sẽ tạo) tự thêm token
        const userProfile = await this.http.get<UserDto>(`${this.API_BASE_URL}/auth/me`).toPromise();

        if (userProfile) {
          localStorage.setItem('app_current_user', JSON.stringify(userProfile));
          this.currentUserSubject.next(userProfile);
          this.isLoggedInSubject.next(true);
          console.log('AuthService: User profile fetched and session established. Navigating to chat.');
          this.router.navigate(['/chat']); // Điều hướng đến trang chính
        } else {
          // Trường hợp API /auth/me trả về lỗi hoặc không có user (dù có token)
          console.error('AuthService: /auth/me did not return a user profile.');
          await this.logoutAndRedirectToLogin('Failed to retrieve user profile after login.');
        }
      } catch (error) {
        console.error('AuthService: Error calling /auth/me or processing user profile', error);
        await this.logoutAndRedirectToLogin('Error verifying session. Please login again.');
      }
    } else {
      console.error('AuthService: Auth callback received no token.');
      await this.logoutAndRedirectToLogin('Authentication failed: No token received from server.');
    }
  }

  private parseRoles(rolesClaim: any): string[] {
    if (!rolesClaim) {
      return [];
    }
    if (Array.isArray(rolesClaim)) {
      return rolesClaim.filter(role => typeof role === 'string');
    }
    if (typeof rolesClaim === 'string') {
      return [rolesClaim];
    }
    return [];
  }

  private async logoutAndRedirectToLogin(errorMessage?: string): Promise<void> {
      localStorage.removeItem('app_auth_token');
      localStorage.removeItem('app_current_user');
      this.isLoggedInSubject.next(false);
      this.currentUserSubject.next(null);
      const queryParams = errorMessage ? { error: encodeURIComponent(errorMessage) } : {};
      this.router.navigate(['/login'], { queryParams });
  }

  async logout(): Promise<void> {
    console.log('Logging out...');
    localStorage.removeItem('app_auth_token');
    localStorage.removeItem('app_current_user');
    this.isLoggedInSubject.next(false);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  public getAuthToken(): string | null {
    return localStorage.getItem('app_auth_token');
  }
}