import { Component, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from './core/services/auth.service'; 
import { SignalRService } from './core/services/signalr.service'; 
import { ChatStateService } from './core/services/chat-state.service'; 

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit { 
  title = 'ChatApp Angular Client';

  // Inject services to ensure they are created
  private authService = inject(AuthService);
  private signalRService = inject(SignalRService);
  private chatStateService = inject(ChatStateService);

  constructor() {

  }

  ngOnInit(): void {
    console.log('AppComponent initialized. AuthService, SignalRService, ChatStateService should be active.');
  }
}