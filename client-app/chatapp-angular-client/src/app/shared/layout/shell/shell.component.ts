// src/app/shared/layout/shell/shell.component.ts
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router'; // Cho routerLink, router-outlet
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu'; // Cho menu user
import { MatDividerModule } from '@angular/material/divider';
import { Observable, Subscription } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service'; // Import AuthService
import { UserDto, LoginResponseDto } from '../../../core/models/auth.dtos'; 

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatSidenavModule,
    MatListModule,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router); // Inject Router

  isLoggedIn$: Observable<boolean>;
  currentUser$: Observable<UserDto | null>;

  constructor() {
    this.isLoggedIn$ = this.authService.isLoggedIn$;
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // (Optional) Subscribe để lấy giá trị trực tiếp nếu cần
    // this.authSubscription = this.authService.isLoggedIn$.subscribe(
    //   loggedIn => this.isLoggedIn = loggedIn
    // );
    // this.authSubscription.add(
    //   this.authService.currentUser$.subscribe(
    //     user => this.currentUser = user
    //   )
    // );
  }

  logout(): void {
    this.authService.logout();
  }

  ngOnDestroy(): void {
  }
}