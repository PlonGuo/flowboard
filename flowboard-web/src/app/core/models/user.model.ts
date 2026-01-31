export interface User {
  id: number;
  email: string;
  fullName: string;
  avatarUrl: string;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface UserProfile {
  name: string;
  avatarUrl: string;
}
