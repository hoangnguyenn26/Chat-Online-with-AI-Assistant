export interface UserDto {
  id: string;
  userName: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  roles: string[];
}

export interface LoginResponseDto {
  token: string;
  user: UserDto;
} 