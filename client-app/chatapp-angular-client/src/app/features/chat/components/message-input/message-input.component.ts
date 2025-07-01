import { Component, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; 
import { MatInputModule } from '@angular/material/input'; 
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TextFieldModule } from '@angular/cdk/text-field'; 
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-message-input',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    TextFieldModule,
    MatTooltipModule
  ],
  templateUrl: './message-input.component.html',
  styleUrl: './message-input.component.scss'
})
export class MessageInputComponent {
  @Output() messageSent = new EventEmitter<{ content: string, isAiQuery: boolean }>();
  newMessageContent: string = '';
  isAiMode = false; // Cờ để biết có đang hỏi AI không

  get inputPlaceholder(): string {
    return this.isAiMode ? 'Ask a question for AI Assistant...' : 'Type a message...';
  }

  sendMessage(): void {
    const content = this.newMessageContent.trim();
    if (content) {
      this.messageSent.emit({ content, isAiQuery: this.isAiMode });
      this.newMessageContent = ''; // Xóa input sau khi gửi
      this.isAiMode = false; // Reset về chế độ chat thường
    }
  }

  // NOTE: Angular template type checking may pass Event instead of KeyboardEvent, so we use 'any' here to avoid linter errors.
  sendMessageOnEnter(event: any): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  onAIAssistClick(): void {
    this.isAiMode = true; // Chuyển sang chế độ hỏi AI
    // Có thể tự động thêm "/ai " vào đầu input nếu muốn
    // if (!this.newMessageContent.startsWith('/ai ')) {
    //   this.newMessageContent = '/ai ';
    // }
  }
}