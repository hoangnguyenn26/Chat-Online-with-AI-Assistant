import { Component, OnInit, OnDestroy, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http'; // Để gọi API Users
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip'; 
import { Observable, Subscription, BehaviorSubject } from 'rxjs';
import { map, distinctUntilChanged } from 'rxjs/operators';
import { UserDto } from '../../../../core/models/auth.dtos';
import { UserPresenceService } from '../../../../core/services/user-presence.service';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatTooltipModule
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private userPresenceService = inject(UserPresenceService);
  private readonly API_USERS_URL = `${environment.apiBaseUrl}/api/users`;

  users: UserDto[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  @Output() userSelected = new EventEmitter<UserDto>();

  // Để highlight user được chọn
  private selectedUserSubject = new BehaviorSubject<UserDto | null>(null);
  public selectedUser$: Observable<UserDto | null> = this.selectedUserSubject.asObservable();


  private presenceSubscription: Subscription | undefined;

  ngOnInit(): void {
    this.loadUsers();

  }

  async loadUsers(): Promise<void> {
    if (this.isLoading) return;
    this.isLoading = true;
    this.errorMessage = null;
    console.log('UserListComponent: Loading users...');

    try {
      const fetchedUsers = await this.http.get<UserDto[]>(this.API_USERS_URL).toPromise();
      if (fetchedUsers) {
        this.users = fetchedUsers.sort((a, b) => a.displayName.localeCompare(b.displayName)); // Sắp xếp theo tên
        console.log('UserListComponent: Users loaded:', this.users);
      } else {
        this.users = [];
      }
    } catch (error: any) {
      console.error('UserListComponent: Error loading users', error);
      this.errorMessage = error?.error?.message || error?.message || 'Failed to load users.';
      this.users = [];
    } finally {
      this.isLoading = false;
    }
  }

  isUserOnline(userId: string): Observable<boolean> {
    return this.userPresenceService.isUserOnline(userId).pipe(
      distinctUntilChanged()
    );
  }

  selectUser(user: UserDto): void {
    console.log('UserListComponent: User selected:', user.displayName);
    this.selectedUserSubject.next(user); // Cập nhật user đang được chọn
    this.userSelected.emit(user);
  }

  trackByUser(index: number, user: UserDto): string {
    return user.id;
  }

  ngOnDestroy(): void {
  }
}