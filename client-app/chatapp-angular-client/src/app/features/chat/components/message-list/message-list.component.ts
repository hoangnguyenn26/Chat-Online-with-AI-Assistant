import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, AfterViewChecked, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PrivateMessageDto } from '../../../../core/models/message.dtos';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-message-list',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule, DatePipe],
  templateUrl: './message-list.component.html',
  styleUrl: './message-list.component.scss'
})
export class MessageListComponent implements AfterViewChecked, OnChanges {
  @Input() messages: PrivateMessageDto[] = [];
  @Input() isLoadingHistory = false; // Cờ cho biết đang tải lịch sử
  @Output() loadMoreHistory = new EventEmitter<void>(); // Event để yêu cầu tải thêm lịch sử

  @ViewChild('messageListContainer') private messageListContainer?: ElementRef<HTMLDivElement>;
  @ViewChild('endOfMessages') private endOfMessages?: ElementRef<HTMLDivElement>;

  private authService = inject(AuthService);
  public currentUserId: string | undefined;
  private shouldScrollToBottom = true; // Cờ để kiểm soát việc cuộn
  private previousScrollHeight = 0;

  constructor() {
    this.authService.currentUser$.subscribe(user => {
      this.currentUserId = user?.id;
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['messages']) {
        // Nếu tin nhắn mới được thêm vào cuối, đánh dấu để cuộn xuống
        // So sánh độ dài hoặc tin nhắn cuối cùng để xác định là thêm mới hay tải lịch sử
        const previousMessages = changes['messages'].previousValue as PrivateMessageDto[];
        const currentMessages = changes['messages'].currentValue as PrivateMessageDto[];

        if (currentMessages && previousMessages && currentMessages.length > previousMessages.length &&
            currentMessages[currentMessages.length-1] !== previousMessages[previousMessages.length-1]) {
             this.shouldScrollToBottom = true;
        } else if (currentMessages && previousMessages && currentMessages.length > previousMessages.length &&
                   currentMessages[0] !== previousMessages[0]) {
             // Lịch sử được thêm vào đầu, không cuộn xuống, giữ vị trí cuộn
             this.shouldScrollToBottom = false;
             // Lưu scrollHeight trước khi DOM cập nhật
             if (this.messageListContainer) {
                 this.previousScrollHeight = this.messageListContainer.nativeElement.scrollHeight;
             }
        } else if (!previousMessages && currentMessages?.length > 0) {
             // Lần load đầu tiên
             this.shouldScrollToBottom = true;
        }
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom && this.messageListContainer) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false; // Reset cờ sau khi cuộn
    } else if (!this.shouldScrollToBottom && this.messageListContainer && this.previousScrollHeight > 0) {
        // Nếu tải lịch sử, khôi phục vị trí cuộn
        const newScrollHeight = this.messageListContainer.nativeElement.scrollHeight;
        this.messageListContainer.nativeElement.scrollTop += (newScrollHeight - this.previousScrollHeight);
        this.previousScrollHeight = 0; // Reset
    }
  }

  private scrollToBottom(): void {
    try {
      if (this.endOfMessages?.nativeElement) {
        this.endOfMessages.nativeElement.scrollIntoView({ behavior: 'smooth' });
      } else if (this.messageListContainer?.nativeElement) {
        this.messageListContainer.nativeElement.scrollTop = this.messageListContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      console.error('Error scrolling to bottom:', err);
    }
  }

  onScroll(): void {
    if (this.messageListContainer?.nativeElement.scrollTop === 0 && !this.isLoadingHistory) {
        console.log('MessageListComponent: Scrolled to top, requesting more history.');
        this.loadMoreHistory.emit();
    }
  }

  trackByMessageId(index: number, message: PrivateMessageDto): string {
    return message.id;
  }

  // Chỉ hiển thị tên người gửi nếu tin nhắn trước đó là của người khác hoặc là tin nhắn đầu tiên
  showSenderName(currentMessage: PrivateMessageDto, allMessages: PrivateMessageDto[], index: number): boolean {
    if (index === 0) return true; // Tin nhắn đầu tiên
    if (allMessages[index - 1].senderId !== currentMessage.senderId) return true; // Người gửi khác
    return false;
  }
}