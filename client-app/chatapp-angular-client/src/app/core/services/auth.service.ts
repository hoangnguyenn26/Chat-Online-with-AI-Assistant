import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http'; 
import { environment } from '../../../environments/environment'; 
import { UserDto, LoginResponseDto } from '../models/auth.dtos';
import { PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_BASE_URL = environment.apiBaseUrl;
  private router = inject(Router);
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private _authToken: string | null = null;
  private _initialized = false;

  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  public isLoggedIn$: Observable<boolean> = this.isLoggedInSubject.asObservable();

  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$: Observable<UserDto | null> = this.currentUserSubject.asObservable();

  constructor() {
    console.log('🔧 AuthService constructor called');
  }

  // Explicit initialization method
  public async init(): Promise<void> {
    if (this._initialized || !this.isBrowser()) {
      console.log('ℹ️ AuthService: Already initialized or not in browser, skipping init.');
      return;
    }
    this._initialized = true;
    console.log('🚀 AuthService initializing...');
    await this.initializeAuthState();
  }

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  private getLocalStorageItem(key: string): string | null {
    return this.isBrowser() ? localStorage.getItem(key) : null;
  }

  private setLocalStorageItem(key: string, value: string): void {
    if (this.isBrowser()) {
      localStorage.setItem(key, value);
    }
  }

  private removeLocalStorageItem(key: string): void {
    if (this.isBrowser()) {
      localStorage.removeItem(key);
    }
  }

  private hasToken(): boolean {
    return !!this.getLocalStorageItem('app_auth_token');
  }

  private getUserFromStorage(): UserDto | null {
    const userJson = this.getLocalStorageItem('app_current_user');
    if (userJson) {
      try {
        return JSON.parse(userJson) as UserDto;
      } catch (e) {
        console.error('Error parsing user from localStorage', e);
        this.removeLocalStorageItem('app_current_user');
        return null;
      }
    }
    return null;
  }

  private async initializeAuthState(): Promise<void> {
    console.log('🔍 Initializing auth state...');
    
    const token = this.getLocalStorageItem('app_auth_token');
    const savedUser = this.getUserFromStorage();
    
    if (token) {
      this._authToken = token;
      console.log('ℹ️ AuthService: Token found in storage. Verifying with /auth/me...');
      
      try {
        const userProfile = await this.http.get<UserDto>(`${this.API_BASE_URL}/auth/me`).toPromise();
        if (userProfile) {
          this.setLocalStorageItem('app_current_user', JSON.stringify(userProfile));
          this.currentUserSubject.next(userProfile);
          this.isLoggedInSubject.next(true);
          console.log('✅ AuthService: Session restored for user:', userProfile.userName);
        } else {
          console.warn('⚠️ AuthService: Token verification failed or no user profile from /auth/me.');
          await this.clearAuthState();
        }
      } catch (error: any) {
        console.error('❌ AuthService: Error verifying token with /auth/me.', error);
        await this.clearAuthState();
        // Avoid navigation here to prevent loops
      }
    } else {
      console.log('ℹ️ AuthService: No token found in storage.');
      await this.clearAuthState();
    }
  }

  private async clearAuthState(): Promise<void> {
    this.removeLocalStorageItem('app_auth_token');
    this.removeLocalStorageItem('app_current_user');
    this._authToken = null;
    this.isLoggedInSubject.next(false);
    this.currentUserSubject.next(null);
  }

  public getCurrentToken(): string | null {
    return this._authToken || this.getLocalStorageItem('app_auth_token');
  }

  loginWithGoogle(): void {
    const apiLoginGoogleUrl = `${this.API_BASE_URL}/auth/login-google`;
    console.log(`🔄 Redirecting to Google login via API: ${apiLoginGoogleUrl}`);
    window.location.href = apiLoginGoogleUrl;
  }

  async handleAuthCallback(token: string): Promise<void> {
    console.log('🔄 AuthService: Handling auth callback with token:', token ? 'Token Received' : 'No Token');
    
    if (!token) {
      console.error('❌ AuthService: Auth callback received no token.');
      await this.logoutAndRedirectToLogin('Authentication failed: No token received from server.');
      return;
    }

    this.setLocalStorageItem('app_auth_token', token);
    this._authToken = token;

    try {
      console.log('🔄 AuthService: Calling /auth/me to fetch user profile...');
      const userProfile = await this.http.get<UserDto>(`${this.API_BASE_URL}/auth/me`).toPromise();

      if (userProfile) {
        this.setLocalStorageItem('app_current_user', JSON.stringify(userProfile));
        this.currentUserSubject.next(userProfile);
        this.isLoggedInSubject.next(true);
        console.log('✅ AuthService: User profile fetched and session established. Navigating to chat.');
        await this.router.navigate(['/chat']);
      } else {
        console.error('❌ AuthService: /auth/me did not return a user profile.');
        await this.logoutAndRedirectToLogin('Failed to retrieve user profile after login.');
      }
    } catch (error) {
      console.error('❌ AuthService: Error calling /auth/me or processing user profile', error);
      await this.logoutAndRedirectToLogin('Error verifying session. Please login again.');
    }
  }

  private async logoutAndRedirectToLogin(errorMessage?: string): Promise<void> {
    await this.clearAuthState();
    const queryParams = errorMessage ? { error: encodeURIComponent(errorMessage) } : {};
    await this.router.navigate(['/login'], { queryParams });
  }

  async logout(): Promise<void> {
    console.log('🔄 Logging out...');
    await this.clearAuthState();
    await this.router.navigate(['/login']);
  }

  public getAuthToken(): string | null {
    return this.getLocalStorageItem('app_auth_token');
  }
}