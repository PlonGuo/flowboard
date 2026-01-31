import { Task } from './task.model';
import { UserProfile } from './user.model';

export interface Column {
  id: string;
  title: string;
  tasks: string[]; // array of task ids
  color: string;
}

export interface Board {
  id: string;
  title: string;
  lastModified: string;
  members: UserProfile[];
  moreMembersCount: number;
  progress: number;
  imageUrl: string;
  columns: Column[];
  tasks: Task[];
}

export interface BoardDto {
  id: number;
  name: string;
  description?: string;
  teamId: number;
  createdById: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface ColumnDto {
  id: number;
  name: string;
  boardId: number;
  position: number;
  wipLimit?: number;
}

export interface CreateBoardRequest {
  name: string;
  description?: string;
  teamId: number;
}

export interface CreateColumnRequest {
  name: string;
  boardId: number;
  wipLimit?: number;
}
