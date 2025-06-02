import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router'; // Nếu dùng router-outlet con
import { Subscription } from 'rxjs';

import { UserListComponent } from '../components/user-list/user-list.component';
import { ChatWindowComponent } from '../chat-window/chat-window.component';
import { UserDto } from '../../../core/models/auth.dtos';
import { ChatStateService } from '../../../core/services/chat-state.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-chat-layout',
  standalone: true,
  imports: [
    CommonModule,
    UserListComponent,
    ChatWindowComponent 
  ],
  templateUrl: './chat-layout.component.html',
  styleUrl: './chat-layout.component.scss'
})
export class ChatLayoutComponent implements OnInit, OnDestroy {
  private chatStateService = inject(ChatStateService);
  private authService = inject(AuthService); // Inject nếu cần

  public selectedPartnerForChatWindow: UserDto | null = null;

  public currentUserId: string | undefined;

  private selectedPartnerSubscription: Subscription | undefined;
  private currentUserSubscription: Subscription | undefined;

  constructor() {}

  ngOnInit(): void {
    this.selectedPartnerSubscription = this.chatStateService.selectedChatPartner$.subscribe(
      (partner) => {
        console.log('ChatLayoutComponent: Selected partner updated in state ->', partner?.displayName);
        this.selectedPartnerForChatWindow = partner;
      }
    );
    this.currentUserSubscription = this.authService.currentUser$.subscribe(user => {
      this.currentUserId = user?.id;
    });
  }

  onChatPartnerSelected(selectedUser: UserDto): void {
    console.log('ChatLayoutComponent: User selected from list component ->', selectedUser.displayName);
    this.chatStateService.setSelectedChatPartner(selectedUser);
  }



  ngOnDestroy(): void {
    this.selectedPartnerSubscription?.unsubscribe();
    this.currentUserSubscription?.unsubscribe();
  }
}