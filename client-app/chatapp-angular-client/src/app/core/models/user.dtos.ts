export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  lastSeenUtc?: string; // ISO string for DateTime
  isOnline: boolean;
  roles: string[];
} 