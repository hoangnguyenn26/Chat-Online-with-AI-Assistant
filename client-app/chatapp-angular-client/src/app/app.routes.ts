import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const appRoutes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth-callback',
    loadComponent: () => import('./features/auth/auth-callback/auth-callback.component').then(m => m.AuthCallbackComponent)
  },
  {
    path: 'chat',
    canActivate: [authGuard],
    loadComponent: () => import('./features/chat/chat-window/chat-window.component').then(m => m.ChatWindowComponent) // Hoặc một ShellComponent
  },
  {
    path: '',
    redirectTo: '/login', 
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: '/login' 
  }
];