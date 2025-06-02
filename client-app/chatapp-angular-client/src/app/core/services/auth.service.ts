// src/app/core/services/auth.service.ts
import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, firstValueFrom } from 'rxjs'; 
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { UserDto, LoginResponseDto } from '../models/auth.dtos';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_BASE_URL = environment.apiBaseUrl;
  private router = inject(Router);
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID); // Inject PLATFORM_ID

  private _authToken: string | null = null;
  private _initialized = false;

  private isLoggedInSubject = new BehaviorSubject<boolean>(false);
  public isLoggedIn$: Observable<boolean> = this.isLoggedInSubject.asObservable();

  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$: Observable<UserDto | null> = this.currentUserSubject.asObservable();

  private readonly TOKEN_KEY = 'app_auth_token';
  private readonly USER_KEY = 'app_current_user';

  constructor() {
    console.log('🔧 AuthService constructor called');
    // Không gọi init tự động ở đây, để app.component hoặc APP_INITIALIZER gọi
  }

  public async init(): Promise<void> {
    if (this._initialized || !this.isBrowser()) {
      console.log(`ℹ️ AuthService: ${this._initialized ? 'Already initialized' : 'Not in browser'}, skipping init.`);
      // Nếu đã init và có token, vẫn phát ra giá trị hiện tại để các subscriber mới nhận được
      if (this._initialized && this.isBrowser()) {
         this.isLoggedInSubject.next(!!this._authToken);
         this.currentUserSubject.next(this.currentUserSubject.value);
      }
      return;
    }
    this._initialized = true;
    console.log('🚀 AuthService initializing application state...');
    await this.initializeAuthState();
  }

  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  private getLocalStorageItem(key: string): string | null {
    if (this.isBrowser()) {
      return localStorage.getItem(key);
    }
    return null;
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

  private async initializeAuthState(): Promise<void> {
    console.log('🔍 AuthService: Initializing auth state from storage...');
    const token = this.getLocalStorageItem(this.TOKEN_KEY);

    if (token) {
      this._authToken = token;
      console.log('ℹ️ AuthService: Token found in storage. Verifying with /auth/me...');
      try {
        const userProfile = await firstValueFrom(this.http.get<UserDto>(`${this.API_BASE_URL}/auth/me`));

        if (userProfile) {
          this.setLocalStorageItem(this.USER_KEY, JSON.stringify(userProfile));
          this.currentUserSubject.next(userProfile);
          this.isLoggedInSubject.next(true);
          console.log('✅ AuthService: Session restored and verified for user:', userProfile.displayName || userProfile.email);
        } else {
          console.warn('⚠️ AuthService: Token verification via /auth/me did not return a user profile. Clearing state.');
          await this.clearAuthStateAndStorage();
        }
      } catch (error: any) {
        console.error('❌ AuthService: Error verifying token with /auth/me. Clearing state.', error instanceof HttpErrorResponse ? error.message : error);
        await this.clearAuthStateAndStorage();
      }
    } else {
      console.log('ℹ️ AuthService: No token found in storage. Ensuring clean state.');
      await this.clearAuthStateAndStorage(false); // Không cần xóa storage vì đã không có gì
    }
  }

  private async clearAuthStateAndStorage(removeFromStorage: boolean = true): Promise<void> {
    if (removeFromStorage && this.isBrowser()) {
      this.removeLocalStorageItem(this.TOKEN_KEY);
      this.removeLocalStorageItem(this.USER_KEY);
    }
    this._authToken = null;
    this.currentUserSubject.next(null);
    this.isLoggedInSubject.next(false); // Quan trọng: Phát ra trạng thái đã logout
    console.log('🔒 AuthService: Auth state cleared.');
  }

  public getCurrentToken(): string | null {
    if (this.isBrowser()) { 
      return this._authToken || localStorage.getItem(this.TOKEN_KEY);
    }
    return this._authToken;
  }

  loginWithGoogle(): void {
    if (!this.isBrowser()) {
      console.warn('AuthService: loginWithGoogle called on server. This should not happen.');
      return;
    }
    const apiLoginGoogleUrl = `${this.API_BASE_URL}/auth/login-google`;
    console.log(`AuthService: Redirecting to Google login via API: ${apiLoginGoogleUrl}`);
    window.location.href = apiLoginGoogleUrl;
  }

  async handleAuthCallback(token: string): Promise<void> {
    console.log('🔄 AuthService: Handling auth callback with token:', token ? 'Token Received' : 'No Token');
    if (!this.isBrowser()) {
        console.warn('AuthService: handleAuthCallback called on server. This should not happen.');
        return;
    }

    if (!token) {
      console.error('❌ AuthService: Auth callback received no token.');
      await this.logoutAndRedirectToLogin('Authentication failed: No token received from server.');
      return;
    }

    this.setLocalStorageItem(this.TOKEN_KEY, token);
    this._authToken = token;
    console.log('ℹ️ AuthService: Token stored in localStorage and service instance.');
    
    try {
      console.log('🔄 AuthService: Calling /auth/me to fetch user profile after callback...');
      const userProfile = await firstValueFrom(this.http.get<UserDto>(`${this.API_BASE_URL}/auth/me`));

      if (userProfile) {
        this.setLocalStorageItem(this.USER_KEY, JSON.stringify(userProfile));
        this.currentUserSubject.next(userProfile);
        this.isLoggedInSubject.next(true); // Phát ra trạng thái đã login
        console.log('✅ AuthService: User profile fetched and session established via callback. Navigating to chat.');
        await this.router.navigate(['/chat']); // Hoặc trang đích sau khi login
      } else {
        console.error('❌ AuthService: /auth/me did not return a user profile after callback.');
        await this.logoutAndRedirectToLogin('Failed to retrieve user profile after successful authentication.');
      }
    } catch (error) {
      console.error('❌ AuthService: Error calling /auth/me or processing user profile during callback', error);
      await this.logoutAndRedirectToLogin('Error verifying your session after authentication. Please try again.');
    }
  }

  private async logoutAndRedirectToLogin(errorMessage?: string): Promise<void> {
  await this.clearAuthStateAndStorage(); 
  if (this.isBrowser()) { 
      const queryParams = errorMessage ? { error: encodeURIComponent(errorMessage) } : {};
      this.router.navigate(['/login'], { queryParams });
  }
  }

  async logout(): Promise<void> {
    console.log(`🔄 AuthService: Logging out user ${this.currentUserSubject.value?.displayName || this.currentUserSubject.value?.email || '(unknown)'}...`);
    await firstValueFrom(this.http.post(`${this.API_BASE_URL}/auth/logout`, {})).catch(err => console.error("Error calling server logout", err));
    await this.clearAuthStateAndStorage();
    if (this.isBrowser()) {
        await this.router.navigate(['/login']);
    }
  }
}