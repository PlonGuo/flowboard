export enum NotificationType {
  Assignment = 'Assignment',
  StatusChange = 'StatusChange',
  Mention = 'Mention',
  Comment = 'Comment',
  DueDate = 'DueDate',
}

export interface Notification {
  id: number;
  userId: number;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  entityType?: string;
  entityId?: number;
  createdAt: Date;
}
