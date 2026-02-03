import { Injectable, inject, signal, computed } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { TaskItemDto } from '../models/task.model';
import {
  TaskMovedEvent,
  ConnectionState,
  CommentAddedEvent,
  CommentUpdatedEvent,
  CommentDeletedEvent,
  NotificationReceivedEvent,
} from '../models/signalr-events.model';

/**
 * Service for managing SignalR real-time connections to the board hub.
 * Provides observables for task events that occur on boards.
 */
@Injectable({ providedIn: 'root' })
export class SignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection: HubConnection | null = null;
  private currentBoardId: number | null = null;

  // Connection state signals
  private readonly connectionStateSignal = signal<ConnectionState>(
    ConnectionState.Disconnected
  );
  readonly connectionState = this.connectionStateSignal.asReadonly();
  readonly isConnected = computed(
    () => this.connectionStateSignal() === ConnectionState.Connected
  );
  readonly connectionId = signal<string | null>(null);

  // Event subjects for board task events
  private readonly taskCreatedSubject = new Subject<TaskItemDto>();
  private readonly taskUpdatedSubject = new Subject<TaskItemDto>();
  private readonly taskMovedSubject = new Subject<TaskMovedEvent>();
  private readonly taskDeletedSubject = new Subject<number>();

  // Event subjects for comment events
  private readonly commentAddedSubject = new Subject<CommentAddedEvent>();
  private readonly commentUpdatedSubject = new Subject<CommentUpdatedEvent>();
  private readonly commentDeletedSubject = new Subject<CommentDeletedEvent>();

  // Event subject for notifications
  private readonly notificationReceivedSubject =
    new Subject<NotificationReceivedEvent>();

  // Public observables for components to subscribe to
  readonly onTaskCreated$: Observable<TaskItemDto> =
    this.taskCreatedSubject.asObservable();
  readonly onTaskUpdated$: Observable<TaskItemDto> =
    this.taskUpdatedSubject.asObservable();
  readonly onTaskMoved$: Observable<TaskMovedEvent> =
    this.taskMovedSubject.asObservable();
  readonly onTaskDeleted$: Observable<number> =
    this.taskDeletedSubject.asObservable();

  // Public observables for comment events
  readonly onCommentAdded$: Observable<CommentAddedEvent> =
    this.commentAddedSubject.asObservable();
  readonly onCommentUpdated$: Observable<CommentUpdatedEvent> =
    this.commentUpdatedSubject.asObservable();
  readonly onCommentDeleted$: Observable<CommentDeletedEvent> =
    this.commentDeletedSubject.asObservable();

  // Public observable for notifications
  readonly onNotificationReceived$: Observable<NotificationReceivedEvent> =
    this.notificationReceivedSubject.asObservable();

  /**
   * Start the SignalR connection to the board hub.
   * Uses JWT token for authentication passed via query string.
   */
  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) {
      console.warn('SignalR: Cannot start connection - no auth token');
      return;
    }

    this.connectionStateSignal.set(ConnectionState.Connecting);

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/board`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(
        environment.production ? LogLevel.Warning : LogLevel.Information
      )
      .build();

    this.setupEventHandlers();
    this.setupConnectionEvents();

    try {
      await this.hubConnection.start();
      this.connectionStateSignal.set(ConnectionState.Connected);
      this.connectionId.set(this.hubConnection.connectionId);
      console.log('SignalR: Connected, ID:', this.hubConnection.connectionId);
    } catch (error) {
      this.connectionStateSignal.set(ConnectionState.Disconnected);
      console.error('SignalR: Connection error:', error);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection.
   */
  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.leaveCurrentBoard();
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionStateSignal.set(ConnectionState.Disconnected);
      this.connectionId.set(null);
      console.log('SignalR: Disconnected');
    }
  }

  /**
   * Join a board group to receive real-time updates for that board.
   */
  async joinBoard(boardId: number): Promise<void> {
    if (
      !this.hubConnection ||
      this.hubConnection.state !== HubConnectionState.Connected
    ) {
      await this.startConnection();
    }

    // Leave previous board if different
    if (this.currentBoardId && this.currentBoardId !== boardId) {
      await this.leaveBoard(this.currentBoardId);
    }

    try {
      await this.hubConnection!.invoke('JoinBoard', boardId);
      this.currentBoardId = boardId;
      console.log('SignalR: Joined board group:', boardId);
    } catch (error) {
      console.error('SignalR: Failed to join board:', error);
      throw error;
    }
  }

  /**
   * Leave a board group to stop receiving updates for that board.
   */
  async leaveBoard(boardId: number): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.invoke('LeaveBoard', boardId);
        if (this.currentBoardId === boardId) {
          this.currentBoardId = null;
        }
        console.log('SignalR: Left board group:', boardId);
      } catch (error) {
        console.error('SignalR: Failed to leave board:', error);
      }
    }
  }

  /**
   * Leave the current board group (if any).
   */
  private async leaveCurrentBoard(): Promise<void> {
    if (this.currentBoardId) {
      await this.leaveBoard(this.currentBoardId);
    }
  }

  /**
   * Set up handlers for SignalR events from the server.
   */
  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('TaskCreated', (task: TaskItemDto) => {
      console.log('SignalR: Received TaskCreated event:', task);
      this.taskCreatedSubject.next(task);
    });

    this.hubConnection.on('TaskUpdated', (task: TaskItemDto) => {
      console.log('SignalR: Received TaskUpdated event:', task);
      this.taskUpdatedSubject.next(task);
    });

    this.hubConnection.on('TaskMoved', (event: TaskMovedEvent) => {
      console.log('SignalR: Received TaskMoved event:', event);
      this.taskMovedSubject.next(event);
    });

    this.hubConnection.on('TaskDeleted', (taskId: number) => {
      console.log('SignalR: Received TaskDeleted event:', taskId);
      this.taskDeletedSubject.next(taskId);
    });

    // Comment events
    this.hubConnection.on('CommentAdded', (event: CommentAddedEvent) => {
      console.log('SignalR: Received CommentAdded event:', event);
      this.commentAddedSubject.next(event);
    });

    this.hubConnection.on('CommentUpdated', (event: CommentUpdatedEvent) => {
      console.log('SignalR: Received CommentUpdated event:', event);
      this.commentUpdatedSubject.next(event);
    });

    this.hubConnection.on('CommentDeleted', (event: CommentDeletedEvent) => {
      console.log('SignalR: Received CommentDeleted event:', event);
      this.commentDeletedSubject.next(event);
    });

    // Notification events (sent to user-specific group)
    this.hubConnection.on(
      'NotificationReceived',
      (event: NotificationReceivedEvent) => {
        console.log('SignalR: Received NotificationReceived event:', event);
        this.notificationReceivedSubject.next(event);
      }
    );
  }

  /**
   * Set up handlers for connection state changes.
   */
  private setupConnectionEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting((error) => {
      this.connectionStateSignal.set(ConnectionState.Reconnecting);
      console.log('SignalR: Reconnecting...', error?.message);
    });

    this.hubConnection.onreconnected((connectionId) => {
      this.connectionStateSignal.set(ConnectionState.Connected);
      this.connectionId.set(connectionId ?? null);
      console.log('SignalR: Reconnected, ID:', connectionId);

      // Rejoin board group after reconnection
      if (this.currentBoardId) {
        this.hubConnection!.invoke('JoinBoard', this.currentBoardId).catch(
          (err) => console.error('SignalR: Failed to rejoin board:', err)
        );
      }
    });

    this.hubConnection.onclose((error) => {
      this.connectionStateSignal.set(ConnectionState.Disconnected);
      this.connectionId.set(null);
      console.log('SignalR: Connection closed', error?.message);
    });
  }
}
