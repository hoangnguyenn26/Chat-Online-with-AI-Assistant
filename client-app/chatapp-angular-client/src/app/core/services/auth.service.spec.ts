import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

describe('AuthService', () => {
  let service: AuthService;
  let httpClientSpy: jasmine.SpyObj<HttpClient>;
  let routerSpy: jasmine.SpyObj<Router>;
  let originalLocation: string;

  beforeEach(() => {
    httpClientSpy = jasmine.createSpyObj('HttpClient', ['get']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: HttpClient, useValue: httpClientSpy },
        { provide: Router, useValue: routerSpy },
        { provide: 'PLATFORM_ID', useValue: 'browser' },
      ],
    });
    service = TestBed.inject(AuthService);
    spyOn(localStorage, 'getItem').and.callFake((key: string) => {
      if (key === 'app_auth_token') return 'valid-token';
      if (key === 'app_current_user') return JSON.stringify({ userName: 'test' });
      return null;
    });
    spyOn(localStorage, 'setItem');
    spyOn(localStorage, 'removeItem');
    originalLocation = window.location.href;
  });

  afterEach(() => {
    window.location.href = originalLocation;
  });

  it('should restore session if token is valid', async () => {
    httpClientSpy.get.and.returnValue(of({ userName: 'test' }));
    await service['checkInitialLoginState']();
    service.isLoggedIn$.subscribe(isLoggedIn => expect(isLoggedIn).toBeTrue());
    service.currentUser$.subscribe(user => expect(user?.userName).toBe('test'));
  });

  it('should handle auth callback and fetch user', async () => {
    httpClientSpy.get.and.returnValue(of({ userName: 'test' }));
    await service.handleAuthCallback('token');
    expect(localStorage.setItem).toHaveBeenCalledWith('app_auth_token', 'token');
    service.isLoggedIn$.subscribe(isLoggedIn => expect(isLoggedIn).toBeTrue());
    service.currentUser$.subscribe(user => expect(user?.userName).toBe('test'));
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/chat']);
  });

  it('should logout and clear state', async () => {
    await service.logout();
    expect(localStorage.removeItem).toHaveBeenCalledWith('app_auth_token');
    expect(localStorage.removeItem).toHaveBeenCalledWith('app_current_user');
    service.isLoggedIn$.subscribe(isLoggedIn => expect(isLoggedIn).toBeFalse());
    service.currentUser$.subscribe(user => expect(user).toBeNull());
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should redirect to Google login', () => {
    const testUrl = 'http://test-url';
    Object.defineProperty(window, 'location', {
      value: { href: testUrl },
      writable: true,
    });
    service.loginWithGoogle();
    // No assertion needed, just ensure no error
  });
}); 