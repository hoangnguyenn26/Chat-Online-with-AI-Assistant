import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { ShellComponent } from './shared/layout/shell/shell.component'; 

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
    // Route gốc cho các trang cần layout Shell
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'chat',
        loadComponent: () => import('./features/chat/chat-window/chat-window.component').then(m => m.ChatWindowComponent)
      },
      {
        path: 'users', // Ví dụ trang danh sách user
        loadComponent: () => import('./features/chat/components/user-list/user-list.component').then(m => m.UserListComponent) // Hoặc một component riêng
      },
      // Thêm các route con khác cần Shell Layout ở đây (Profile, Settings...)
      {
        path: '',
        redirectTo: 'chat',
        pathMatch: 'full'
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];