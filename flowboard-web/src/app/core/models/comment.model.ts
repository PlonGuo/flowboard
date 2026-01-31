import { User } from './user.model';

export interface Comment {
  id: number;
  content: string;
  taskId: number;
  authorId: number;
  author?: User;
  isEdited: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateCommentRequest {
  content: string;
  taskId: number;
}

export interface UpdateCommentRequest {
  id: number;
  content: string;
}
