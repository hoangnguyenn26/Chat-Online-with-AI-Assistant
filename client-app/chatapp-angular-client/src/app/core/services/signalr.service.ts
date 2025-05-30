import { Injectable, inject, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service'; 

export interface UserPresenceEvent {
  userId: string;
  isOnline: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService implements OnDestroy {
  private hubConnection?: signalR.HubConnection;
  private readonly API_BASE_URL = environment.apiBaseUrl;
  private readonly HUB_URL = `${this.API_BASE_URL}/chathub`;

  private authService = inject(AuthService);

  private userPresenceChangedSubject = new Subject<UserPresenceEvent>();
  public userPresenceChanged$ = this.userPresenceChangedSubject.asObservable();

  private connectionStateSubject = new BehaviorSubject<boolean>(false);
  public isConnected$ = this.connectionStateSubject.asObservable();

  constructor() {
    this.authService.isLoggedIn$.subscribe(loggedIn => {
      if (loggedIn && !this.hubConnection?.state) { // Chỉ kết nối nếu đã login và chưa có kết nối
        this.startConnection();
      } else if (!loggedIn && this.hubConnection?.state === signalR.HubConnectionState.Connected) {
        this.stopConnection();
      }
    });
  }

  public startConnection(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected ||
        this.hubConnection?.state === signalR.HubConnectionState.Connecting) {
      console.log('SignalRService: Connection already established or connecting.');
      return;
    }

    const token = this.authService.getCurrentToken();

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.HUB_URL, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000])
      .configureLogging(signalR.LogLevel.Information) 
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalRService: Connection started successfully to ChatHub.');
        this.connectionStateSubject.next(true);
        this.registerServerEvents();
      })
      .catch(err => {
        console.error('SignalRService: Error while starting connection: ' + err);
        this.connectionStateSubject.next(false);
        setTimeout(() => this.startConnection(), 5000);
      });

    this.hubConnection.onclose(error => {
      console.warn(`SignalRService: Connection closed. Error: ${error}`);
      this.connectionStateSubject.next(false);
    });

    this.hubConnection.onreconnecting(error => {
      console.info(`SignalRService: Connection reconnecting. Error: ${error}`);
      this.connectionStateSubject.next(false); // Coi như đang offline
    });

    this.hubConnection.onreconnected(connectionId => {
      console.info(`SignalRService: Connection reconnected with ID: ${connectionId}`);
      this.connectionStateSubject.next(true);
    });
  }

  private registerServerEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('UserOnline', (userId: string) => {
      console.log(`SignalRService: User ${userId} is online.`);
      this.userPresenceChangedSubject.next({ userId, isOnline: true });
    });

    this.hubConnection.on('UserOffline', (userId: string) => {
      console.log(`SignalRService: User ${userId} is offline.`);
      this.userPresenceChangedSubject.next({ userId, isOnline: false });
    });

  }

  public stopConnection(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.stop()
        .then(() => {
          console.log('SignalRService: Connection stopped.');
          this.connectionStateSubject.next(false);
        })
        .catch(err => console.error('SignalRService: Error while stopping connection: ' + err));
    }
  }

  public invokeHubMethod(methodName: string, ...args: any[]): Promise<any> | undefined {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return this.hubConnection.invoke(methodName, ...args)
        .catch(err => console.error(`SignalRService: Error invoking ${methodName}: `, err));
    } else {
      console.warn(`SignalRService: Cannot invoke ${methodName}. Connection not established.`);
      return undefined;
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}