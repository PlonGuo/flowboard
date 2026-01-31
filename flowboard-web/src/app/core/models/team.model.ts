import { User } from './user.model';

export enum TeamRole {
  Owner = 'Owner',
  Member = 'Member',
}

export interface Team {
  id: number;
  name: string;
  description?: string;
  ownerId: number;
  owner?: User;
  createdAt: Date;
  updatedAt: Date;
}

export interface TeamMember {
  id: number;
  teamId: number;
  userId: number;
  role: TeamRole;
  joinedAt: Date;
  user?: User;
}
