import { UserSummaryDto } from './board.model';

export enum TeamRole {
  Owner = 'Owner',
  Member = 'Member',
}

export interface TeamDto {
  id: number;
  name: string;
  description?: string;
  inviteCode: string;
  owner: UserSummaryDto;
  memberCount: number;
  currentUserRole?: TeamRole;
  createdAt: string;
  updatedAt: string;
}

export interface TeamMemberDto {
  id: number;
  teamId: number;
  user: UserSummaryDto;
  role: TeamRole;
  joinedAt: string;
}

export interface CreateTeamRequest {
  name: string;
  description?: string;
}

export interface JoinTeamRequest {
  inviteCode: string;
}
