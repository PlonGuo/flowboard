import { Task, TaskApiDto } from './task.model';
import { UserProfile } from './user.model';

// Legacy interfaces for backward compatibility with existing mock data
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

// API DTOs matching backend responses
export interface UserSummaryDto {
  id: number;
  fullName: string;
  email: string;
  avatarUrl?: string;
}

export interface BoardDto {
  id: number;
  name: string;
  description?: string;
  teamId: number;
  teamName: string;
  createdBy: UserSummaryDto;
  createdAt: string;
  updatedAt: string;
}

export interface BoardDetailDto {
  id: number;
  name: string;
  description?: string;
  teamId: number;
  teamName: string;
  createdBy: UserSummaryDto;
  columns: ColumnApiDto[];
  createdAt: string;
  updatedAt: string;
}

export interface BoardSummaryDto {
  id: number;
  name: string;
  description?: string;
  taskCount: number;
  columnCount: number;
  updatedAt: string;
}

export interface ColumnApiDto {
  id: number;
  name: string;
  position: number;
  wipLimit?: number;
  tasks: TaskApiDto[];
}

// Legacy ColumnDto for backward compatibility
export interface ColumnDto {
  id: number;
  name: string;
  boardId: number;
  position: number;
  wipLimit?: number;
}

// Request DTOs
export interface CreateBoardRequest {
  name: string;
  description?: string;
  teamId: number;
}

export interface UpdateBoardRequest {
  name: string;
  description?: string;
}

export interface CreateColumnRequest {
  name: string;
  boardId: number;
  wipLimit?: number;
}

// API Error response
export interface ApiError {
  message: string;
  details?: { field: string; message: string }[];
}
