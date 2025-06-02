import { Injectable, OnDestroy, inject } from '@angular/core';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { PrivateMessageDto } from '../models/message.dtos';
import { UserDto } from '../models/user.dtos';
import { SignalRService } from './signalr.service';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http'; // For API calls to get history
import { environment } from '../../../environments/environment';

interface ChatConversations {
  [partnerUserId: string]: PrivateMessageDto[];
}

@Injectable({
  providedIn: 'root'
})
export class ChatStateService implements OnDestroy {
  private signalRService = inject(SignalRService);
  private authService = inject(AuthService);
  private http = inject(HttpClient);
  private readonly API_BASE_URL = environment.apiBaseUrl;

  private selectedChatPartnerSubject = new BehaviorSubject<UserDto | null>(null);
  public selectedChatPartner$: Observable<UserDto | null> = this.selectedChatPartnerSubject.asObservable();

  private conversationsSubject = new BehaviorSubject<ChatConversations>({});

  private currentChatMessagesSubject = new BehaviorSubject<PrivateMessageDto[]>([]);
  public currentChatMessages$: Observable<PrivateMessageDto[]> = this.currentChatMessagesSubject.asObservable();

  private messageSubscription: Subscription | undefined;
  private selectedPartnerSubscription: Subscription | undefined;
  private currentUserId: string | undefined;

  constructor() {
    this.authService.currentUser$.subscribe(user => {
        this.currentUserId = user?.id;
        if (!user) { // Clear state on logout
            this.conversationsSubject.next({});
            this.currentChatMessagesSubject.next([]);
            this.selectedChatPartnerSubject.next(null);
        }
    });

    this.messageSubscription = this.signalRService.privateMessageReceived$.subscribe(
      (message) => {
        this.addMessageToConversation(message);
      }
    );

    this.selectedPartnerSubscription = this.selectedChatPartner$.subscribe(partner => {
      if (partner) {
        const conversation = this.conversationsSubject.value[partner.id] || [];
        this.currentChatMessagesSubject.next([...conversation]);
        if (conversation.length === 0) {
             this.loadMessageHistory(partner.id);
        }
      } else {
        this.currentChatMessagesSubject.next([]);
      }
    });
  }

  public setSelectedChatPartner(partner: UserDto | null): void {
    console.log('ChatStateService: Selected chat partner:', partner?.displayName);
    this.selectedChatPartnerSubject.next(partner);
  }

  private addMessageToConversation(message: PrivateMessageDto): void {
    if (!this.currentUserId) return;

    const partnerId = message.senderId === this.currentUserId ? message.receiverId : message.senderId;
    if (!partnerId) {
        console.error("ChatStateService: Could not determine partnerId for message:", message);
        return;
    }

    const currentConversations = { ...this.conversationsSubject.value };
    if (!currentConversations[partnerId]) {
      currentConversations[partnerId] = [];
    }

    if (!currentConversations[partnerId].find(m => m.id === message.id)) {
        currentConversations[partnerId].push(message);
        currentConversations[partnerId].sort((a, b) => new Date(a.timestampUtc).getTime() - new Date(b.timestampUtc).getTime());
    }

    this.conversationsSubject.next(currentConversations);

    if (this.selectedChatPartnerSubject.value?.id === partnerId) {
      this.currentChatMessagesSubject.next([...currentConversations[partnerId]]);
    }
  }

  public async loadMessageHistory(partnerId: string, beforeTimestamp?: string, limit: number = 20): Promise<boolean> {
    if (!this.currentUserId) {
        console.warn('ChatStateService: Cannot load history, current user not set.');
        return false;
    }
    console.log(`ChatStateService: Loading message history with ${partnerId}, before: ${beforeTimestamp}`);

    let url = `${this.API_BASE_URL}/api/private-messages/${partnerId}?limit=${limit}`;
    if (beforeTimestamp) {
        url += `&beforeTimestamp=${encodeURIComponent(beforeTimestamp)}`;
    }

    try {
        const history = await this.http.get<PrivateMessageDto[]>(url).toPromise(); // Assumes API returns array directly
        if (history && history.length > 0) {
            const currentConversations = { ...this.conversationsSubject.value };
            if (!currentConversations[partnerId]) {
                currentConversations[partnerId] = [];
            }

            // Prepend history messages and ensure no duplicates, then sort
            const existingIds = new Set(currentConversations[partnerId].map(m => m.id));
            const newMessages = history.filter(m => !existingIds.has(m.id));
            currentConversations[partnerId] = [...newMessages, ...currentConversations[partnerId]];
            currentConversations[partnerId].sort((a, b) => new Date(a.timestampUtc).getTime() - new Date(b.timestampUtc).getTime());

            this.conversationsSubject.next(currentConversations);

            if (this.selectedChatPartnerSubject.value?.id === partnerId) {
                this.currentChatMessagesSubject.next([...currentConversations[partnerId]]);
            }
            console.log(`ChatStateService: Loaded ${history.length} history messages for ${partnerId}.`);
            return history.length === limit; // Returns true if more messages might be available
        }
        return false; // No more messages or empty history
    } catch (error) {
        console.error(`ChatStateService: Error loading message history for ${partnerId}`, error);
        return false;
    }
  }

  public clearChatStateForPartner(partnerId: string): void {
    const currentConversations = { ...this.conversationsSubject.value };
    if (currentConversations[partnerId]) {
      delete currentConversations[partnerId];
      this.conversationsSubject.next(currentConversations);
      if (this.selectedChatPartnerSubject.value?.id === partnerId) {
        this.currentChatMessagesSubject.next([]);
      }
      console.log(`ChatStateService: Cleared chat state for partner ${partnerId}`);
    }
  }

  ngOnDestroy(): void {
    this.messageSubscription?.unsubscribe();
    this.selectedPartnerSubscription?.unsubscribe();
    this.conversationsSubject.complete();
    this.currentChatMessagesSubject.complete();
    this.selectedChatPartnerSubject.complete();
  }
}