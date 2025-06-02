import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Observable, Subscription } from 'rxjs';
import { UserDto } from '../../../core/models/user.dtos';
import { PrivateMessageDto } from '../../../core/models/message.dtos';
import { ChatStateService } from '../../../core/services/chat-state.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { MessageListComponent } from '../components/message-list/message-list.component';
import { MessageInputComponent } from '../components/message-input/message-input.component'; 
import { Router } from '@angular/router';

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MessageListComponent,
    MessageInputComponent
  ],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.scss'
})
export class ChatWindowComponent implements OnInit, OnDestroy {
  private chatStateService = inject(ChatStateService);
  private signalRService = inject(SignalRService);
  private authService = inject(AuthService);
  private router = inject(Router);

  selectedChatPartner$: Observable<UserDto | null>;
  currentMessages$: Observable<PrivateMessageDto[]>;
  currentUserId: string | undefined;

  isLoadingHistory = false;

  private subscriptions = new Subscription();

  constructor() {
    this.selectedChatPartner$ = this.chatStateService.selectedChatPartner$;
    this.currentMessages$ = this.chatStateService.currentChatMessages$;
  }

  ngOnInit(): void {
    this.subscriptions.add(
      this.authService.currentUser$.subscribe(user => {
        this.currentUserId = user?.id;
      })
    );
  }

  onMessageSent(content: string): void {
    this.chatStateService.selectedChatPartner$.subscribe(partner => {
      if (partner && content) {
        console.log(`ChatWindow: Sending message to ${partner.displayName}: ${content}`);
        this.signalRService.sendPrivateMessage(partner.id, content)
          .then(() => console.log('ChatWindow: Message sent promise resolved.'))
          .catch(err => {
            console.error('ChatWindow: Failed to send message via SignalR', err);
          });
      }
    }).unsubscribe();
  }

  async loadMoreMessages(): Promise<void> {
    this.chatStateService.selectedChatPartner$.subscribe(partner => {
      if (partner && !this.isLoadingHistory) {
        this.isLoadingHistory = true;
        console.log(`ChatWindow: Requesting more message history for ${partner.displayName}`);
        this.chatStateService.currentChatMessages$.subscribe(messages => {
          const oldestMessageTimestamp = messages.length > 0 ? messages[0].timestampUtc : undefined;
          this.chatStateService.loadMessageHistory(partner.id, oldestMessageTimestamp).then(() => {
            this.isLoadingHistory = false;
          });
        }).unsubscribe();
      }
    }).unsubscribe();
  }

  goBackToList(): void {
    this.chatStateService.setSelectedChatPartner(null);
    console.log("ChatWindow: Go back to user list triggered (implement navigation if needed).");
  }

  get isMobileView(): boolean {
    return window.innerWidth < 768; 
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}