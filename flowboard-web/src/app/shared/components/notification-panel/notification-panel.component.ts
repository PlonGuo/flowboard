import { Component, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import {
  NotificationDto,
  NotificationType,
} from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-panel.component.html',
})
export class NotificationPanelComponent {
  private readonly router = inject(Router);
  readonly notificationService = inject(NotificationService);

  @Output() closePanel = new EventEmitter<void>();

  onNotificationClick(notification: NotificationDto): void {
    // Mark as read
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }

    // Navigate to related content
    if (notification.relatedTaskId && notification.relatedBoardId) {
      this.router.navigate(['/boards', notification.relatedBoardId], {
        queryParams: { taskId: notification.relatedTaskId },
      });
      this.closePanel.emit();
    } else if (notification.relatedBoardId) {
      this.router.navigate(['/boards', notification.relatedBoardId]);
      this.closePanel.emit();
    }
  }

  onMarkAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  onDeleteNotification(event: Event, notificationId: number): void {
    event.stopPropagation();
    this.notificationService.deleteNotification(notificationId).subscribe();
  }

  getNotificationIcon(type: NotificationType): string {
    switch (type) {
      case NotificationType.Mentioned:
        return 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z';
      case NotificationType.TaskAssigned:
        return 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4';
      case NotificationType.TaskStatusChanged:
        return 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z';
      case NotificationType.TaskDueDateApproaching:
        return 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z';
      case NotificationType.CommentAdded:
        return 'M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z';
      case NotificationType.BoardInvitation:
      case NotificationType.TeamInvitation:
        return 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z';
      default:
        return 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9';
    }
  }

  getIconBgColor(type: NotificationType): string {
    switch (type) {
      case NotificationType.Mentioned:
        return 'bg-blue-500';
      case NotificationType.TaskAssigned:
        return 'bg-green-500';
      case NotificationType.TaskStatusChanged:
        return 'bg-purple-500';
      case NotificationType.TaskDueDateApproaching:
        return 'bg-orange-500';
      case NotificationType.CommentAdded:
        return 'bg-cyan-500';
      case NotificationType.BoardInvitation:
      case NotificationType.TeamInvitation:
        return 'bg-indigo-500';
      default:
        return 'bg-gray-500';
    }
  }

  formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffSeconds = Math.floor(diffMs / 1000);
    const diffMinutes = Math.floor(diffSeconds / 60);
    const diffHours = Math.floor(diffMinutes / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffSeconds < 60) {
      return 'Just now';
    } else if (diffMinutes < 60) {
      return `${diffMinutes}m ago`;
    } else if (diffHours < 24) {
      return `${diffHours}h ago`;
    } else if (diffDays < 7) {
      return `${diffDays}d ago`;
    } else {
      return date.toLocaleDateString();
    }
  }
}
