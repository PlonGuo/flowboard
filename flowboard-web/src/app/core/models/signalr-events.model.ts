import { CommentDto } from './task.model';
import { NotificationDto } from './notification.model';

/**
 * SignalR real-time event models
 */

/**
 * Event payload when a task is moved between columns or positions.
 */
export interface TaskMovedEvent {
  taskId: number;
  fromColumnId: number;
  toColumnId: number;
  newPosition: number;
}

/**
 * Event payload when a comment is added to a task.
 */
export interface CommentAddedEvent {
  taskId: number;
  comment: CommentDto;
}

/**
 * Event payload when a comment is updated.
 */
export interface CommentUpdatedEvent {
  taskId: number;
  comment: CommentDto;
}

/**
 * Event payload when a comment is deleted.
 */
export interface CommentDeletedEvent {
  taskId: number;
  commentId: number;
}

/**
 * Event payload when a notification is received.
 */
export interface NotificationReceivedEvent {
  notification: NotificationDto;
}

/**
 * Connection state for the SignalR hub.
 */
export enum ConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
}
