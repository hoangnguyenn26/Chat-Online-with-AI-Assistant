// src/app/core/services/auth.service.ts
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
  private http = inject(HttpClient); // Sẽ dùng sau

  // BehaviorSubject để quản lý trạng thái đăng nhập và thông tin user
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$: Observable<boolean> = this.isLoggedInSubject.asObservable();

  private currentUserSubject = new BehaviorSubject<UserDto | null>(this.getUserFromStorage());
  public currentUser$: Observable<UserDto | null> = this.currentUserSubject.asObservable();

  constructor() {
    // Kiểm tra token khi service khởi tạo
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
        // TODO: (Ngày 12) Gọi API /auth/me để xác thực token và lấy thông tin user mới nhất
        // Hiện tại, chỉ load từ localStorage
        this.isLoggedInSubject.next(true);
        this.currentUserSubject.next(this.getUserFromStorage());
    } else {
        this.isLoggedInSubject.next(false);
        this.currentUserSubject.next(null);
    }
  }


  loginWithGoogle(): void {
    const currentAppUrl = window.location.origin; // Ví dụ: http://localhost:4200
    const apiLoginGoogleUrl = `${this.API_BASE_URL}/auth/login-google`;

    console.log(`Redirecting to Google login via API: ${apiLoginGoogleUrl}`);
    window.location.href = apiLoginGoogleUrl; // Thực hiện redirect
  }

  // Sẽ được gọi từ AuthCallbackComponent
  async handleAuthCallback(token: string): Promise<void> {
    console.log('Handling auth callback with token:', token ? 'Token Received' : 'No Token');
    if (token) {
      localStorage.setItem('app_auth_token', token);
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const user: UserDto = {
          id: payload.uid || payload.sub,
          userName: payload.nameid || payload.name || payload.email.split('@')[0],
          email: payload.email,
          displayName: payload.name || (payload.given_name && payload.family_name ? `${payload.given_name} ${payload.family_name}`: payload.email.split('@')[0]),
          avatarUrl: payload.avatar,
          roles: Array.isArray(payload.role) ? payload.role : (payload.role ? [payload.role] : [])
        };
        localStorage.setItem('app_current_user', JSON.stringify(user));
        this.currentUserSubject.next(user);
        this.isLoggedInSubject.next(true);
        console.log('User processed from token, navigating to chat.');
        this.router.navigate(['/chat']); // Điều hướng đến trang chính sau khi login
      } catch (e) {
        console.error('Error decoding token or setting user:', e);
        await this.logout(); // Nếu lỗi thì logout
      }
    } else {
      console.error('Auth callback received no token.');
      this.isLoggedInSubject.next(false);
      this.currentUserSubject.next(null);
      this.router.navigate(['/login']);
    }
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