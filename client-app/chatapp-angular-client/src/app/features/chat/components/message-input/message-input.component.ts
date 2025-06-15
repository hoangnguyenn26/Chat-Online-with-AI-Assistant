import { Component, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { MatInputModule } from '@angular/material/input'; 
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TextFieldModule } from '@angular/cdk/text-field'; 

@Component({
  selector: 'app-message-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    TextFieldModule
  ],
  templateUrl: './message-input.component.html',
  styleUrl: './message-input.component.scss'
})
export class MessageInputComponent {
  @Output() messageSent = new EventEmitter<string>();
  newMessageContent: string = '';

  sendMessage(): void {
    const content = this.newMessageContent.trim();
    if (content) {
      this.messageSent.emit(content);
      this.newMessageContent = ''; // Xóa input sau khi gửi
    }
  }

  sendMessageOnEnter(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) { // Gửi khi nhấn Enter, không gửi khi Shift+Enter
      event.preventDefault(); // Ngăn xuống dòng mặc định của Enter trong textarea
      this.sendMessage();
    }
  }
}