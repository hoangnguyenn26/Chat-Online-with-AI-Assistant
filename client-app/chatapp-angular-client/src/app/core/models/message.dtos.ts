export interface PrivateMessageDto {
  id: string;
  senderId: string;
  senderDisplayName: string;
  senderAvatarUrl?: string;
  receiverId: string;
  content: string;
  timestampUtc: string; // ISO string for DateTime
  isFromAI: boolean;
  isRead: boolean;
} 