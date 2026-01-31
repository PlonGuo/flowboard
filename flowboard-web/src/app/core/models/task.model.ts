import { UserProfile } from './user.model';

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
}

export interface TaskTag {
  name: string;
  color: string;
  textColor: string;
}

export interface ChecklistItem {
  item: string;
  completed: boolean;
}

export interface Task {
  id: string;
  title: string;
  description: string;
  tags: TaskTag[];
  assignees: UserProfile[];
  commentsCount: number;
  attachmentsCount: number;
  dueDate?: string;
  progress?: number;
  completed: boolean;
  status: string;
  checklist: ChecklistItem[];
  priority?: TaskPriority;
}

export interface TaskDto {
  id: number;
  title: string;
  description?: string;
  columnId: number;
  position: number;
  assigneeId?: number;
  priority: TaskPriority;
  dueDate?: Date;
  createdById: number;
  createdAt: Date;
  updatedAt: Date;
  rowVersion: string;
}

// API response DTO for tasks within board detail
export interface TaskApiDto {
  id: number;
  title: string;
  description?: string;
  position: number;
  priority: TaskPriority;
  dueDate?: string;
  assignee?: {
    id: number;
    fullName: string;
    avatarUrl?: string;
  };
  commentsCount: number;
  attachmentsCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  columnId: number;
  priority?: TaskPriority;
  assigneeId?: number;
  dueDate?: Date;
}

export interface MoveTaskRequest {
  taskId: number;
  toColumnId: number;
  toPosition: number;
  rowVersion: string;
}
