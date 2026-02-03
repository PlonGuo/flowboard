import {
  Injectable,
  signal,
  computed,
  inject,
  OnDestroy,
  DestroyRef,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { environment } from '../../../environments/environment';
import { NotificationDto } from '../models/notification.model';
import { SignalRService } from './signalr.service';

export interface UnreadCountResponse {
  count: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private readonly http = inject(HttpClient);
  private readonly signalRService = inject(SignalRService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly apiUrl = `${environment.apiUrl}/notifications`;

  // State signals
  private readonly notificationsSignal = signal<NotificationDto[]>([]);
  private readonly loadingSignal = signal<boolean>(false);
  private readonly totalCountSignal = signal<number>(0);

  // Public readonly signals
  readonly notifications = this.notificationsSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly totalCount = this.totalCountSignal.asReadonly();

  // Computed signals
  readonly unreadCount = computed(
    () => this.notificationsSignal().filter((n) => !n.isRead).length
  );
  readonly hasUnread = computed(() => this.unreadCount() > 0);
  readonly unreadNotifications = computed(() =>
    this.notificationsSignal().filter((n) => !n.isRead)
  );
  readonly readNotifications = computed(() =>
    this.notificationsSignal().filter((n) => n.isRead)
  );

  constructor() {
    this.subscribeToRealTimeNotifications();
  }

  ngOnDestroy(): void {
    // Cleanup handled by takeUntilDestroyed
  }

  /**
   * Subscribe to real-time notification events from SignalR.
   */
  private subscribeToRealTimeNotifications(): void {
    this.signalRService.onNotificationReceived$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((event) => {
        // Add new notification to the beginning of the list
        this.notificationsSignal.update((notifications) => [
          event.notification,
          ...notifications,
        ]);
        this.totalCountSignal.update((count) => count + 1);
      });
  }

  /**
   * Load notifications for the current user.
   */
  loadNotifications(
    includeRead: boolean = true,
    skip: number = 0,
    take: number = 50
  ): Observable<NotificationDto[]> {
    this.loadingSignal.set(true);
    const params = new URLSearchParams({
      includeRead: String(includeRead),
      skip: String(skip),
      take: String(take),
    });

    return this.http
      .get<NotificationDto[]>(`${this.apiUrl}?${params}`)
      .pipe(
        tap((notifications) => {
          if (skip === 0) {
            // Reset if loading from the beginning
            this.notificationsSignal.set(notifications);
          } else {
            // Append for pagination
            this.notificationsSignal.update((current) => [
              ...current,
              ...notifications,
            ]);
          }
          this.totalCountSignal.set(notifications.length);
          this.loadingSignal.set(false);
        }),
        catchError((error) => {
          this.loadingSignal.set(false);
          return throwError(() => error);
        })
      );
  }

  /**
   * Get the unread notification count.
   */
  getUnreadCount(): Observable<UnreadCountResponse> {
    return this.http.get<UnreadCountResponse>(`${this.apiUrl}/unread-count`);
  }

  /**
   * Mark a single notification as read.
   */
  markAsRead(notificationId: number): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${notificationId}/read`, {})
      .pipe(
        tap(() => {
          this.notificationsSignal.update((notifications) =>
            notifications.map((n) =>
              n.id === notificationId
                ? { ...n, isRead: true, readAt: new Date().toISOString() }
                : n
            )
          );
        })
      );
  }

  /**
   * Mark all notifications as read.
   */
  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        const now = new Date().toISOString();
        this.notificationsSignal.update((notifications) =>
          notifications.map((n) => ({ ...n, isRead: true, readAt: now }))
        );
      })
    );
  }

  /**
   * Delete a notification.
   */
  deleteNotification(notificationId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${notificationId}`).pipe(
      tap(() => {
        this.notificationsSignal.update((notifications) =>
          notifications.filter((n) => n.id !== notificationId)
        );
        this.totalCountSignal.update((count) => Math.max(0, count - 1));
      })
    );
  }

  /**
   * Clear all notifications from local state (does not delete from server).
   */
  clearLocalState(): void {
    this.notificationsSignal.set([]);
    this.totalCountSignal.set(0);
  }
}
