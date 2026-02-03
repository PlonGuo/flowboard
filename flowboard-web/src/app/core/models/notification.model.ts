export enum NotificationType {
  TaskAssigned = 'TaskAssigned',
  TaskStatusChanged = 'TaskStatusChanged',
  TaskDueDateApproaching = 'TaskDueDateApproaching',
  Mentioned = 'Mentioned',
  CommentAdded = 'CommentAdded',
  BoardInvitation = 'BoardInvitation',
  TeamInvitation = 'TeamInvitation',
}

export interface NotificationDto {
  id: number;
  userId: number;
  type: NotificationType;
  title: string;
  message: string;
  relatedTaskId?: number;
  relatedBoardId?: number;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}
