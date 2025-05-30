import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { SignalRService, UserPresenceEvent } from './signalr.service';

export interface UserOnlineStatus {
  [userId: string]: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserPresenceService implements OnDestroy {
  private onlineUsersMap = new BehaviorSubject<UserOnlineStatus>({});
  public onlineUsersMap$: Observable<UserOnlineStatus> = this.onlineUsersMap.asObservable();

  private subscriptions = new Subscription();

  constructor(private signalRService: SignalRService) {
    this.subscriptions.add(
      this.signalRService.userPresenceChanged$.subscribe(
        (event: UserPresenceEvent) => this.updateUserStatus(event.userId, event.isOnline)
      )
    );
    this.subscriptions.add(
      this.signalRService.isConnected$.subscribe()
    );
  }

  private updateUserStatus(userId: string, isOnline: boolean): void {
    const currentMap = { ...this.onlineUsersMap.value };
    currentMap[userId] = isOnline;
    this.onlineUsersMap.next(currentMap);
  }

  public isUserOnline(userId: string): Observable<boolean> {
    return this.onlineUsersMap.pipe(
      map(usersMap => !!usersMap[userId])
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}