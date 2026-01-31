export interface User {
  id: number;
  email: string;
  fullName: string;
  avatarUrl: string | null;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface UserProfile {
  name: string;
  avatarUrl: string;
}
