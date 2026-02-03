import { Injectable, inject, signal, computed } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import {
  CanvasUser,
  SceneUpdateEvent,
  PointerUpdateEvent,
  CanvasUpdatedEvent,
  CanvasDeletedEvent,
} from '../models/canvas.model';

export enum CanvasConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
}

@Injectable({ providedIn: 'root' })
export class CanvasSignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection: HubConnection | null = null;
  private currentCanvasId: number | null = null;

  // Connection state
  private readonly connectionStateSignal = signal<CanvasConnectionState>(
    CanvasConnectionState.Disconnected
  );
  readonly connectionState = this.connectionStateSignal.asReadonly();
  readonly isConnected = computed(
    () => this.connectionStateSignal() === CanvasConnectionState.Connected
  );

  // Online users in current canvas
  private readonly onlineUsersSignal = signal<CanvasUser[]>([]);
  readonly onlineUsers = this.onlineUsersSignal.asReadonly();

  // Connection ID for excluding self from broadcasts
  get connectionId(): string | null {
    return this.hubConnection?.connectionId ?? null;
  }

  // Event subjects
  private readonly sceneUpdatedSubject = new Subject<SceneUpdateEvent>();
  private readonly pointerUpdatedSubject = new Subject<PointerUpdateEvent>();
  private readonly userJoinedSubject = new Subject<CanvasUser>();
  private readonly userLeftSubject = new Subject<string>();
  private readonly canvasUpdatedSubject = new Subject<CanvasUpdatedEvent>();
  private readonly canvasDeletedSubject = new Subject<CanvasDeletedEvent>();
  private readonly canvasSavedSubject = new Subject<{ canvasId: number; version: number }>();

  // Public observables
  readonly onSceneUpdated$: Observable<SceneUpdateEvent> =
    this.sceneUpdatedSubject.asObservable();
  readonly onPointerUpdated$: Observable<PointerUpdateEvent> =
    this.pointerUpdatedSubject.asObservable();
  readonly onUserJoined$: Observable<CanvasUser> =
    this.userJoinedSubject.asObservable();
  readonly onUserLeft$: Observable<string> =
    this.userLeftSubject.asObservable();
  readonly onCanvasUpdated$: Observable<CanvasUpdatedEvent> =
    this.canvasUpdatedSubject.asObservable();
  readonly onCanvasDeleted$: Observable<CanvasDeletedEvent> =
    this.canvasDeletedSubject.asObservable();
  readonly onCanvasSaved$: Observable<{ canvasId: number; version: number }> =
    this.canvasSavedSubject.asObservable();

  async startConnection(): Promise<void> {
    if (this.hubConnection) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) {
      console.warn('No auth token available for canvas SignalR connection');
      return;
    }

    this.connectionStateSignal.set(CanvasConnectionState.Connecting);

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/canvas`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(
        environment.production ? LogLevel.Warning : LogLevel.Information
      )
      .build();

    this.setupEventHandlers();
    this.setupConnectionHandlers();

    try {
      await this.hubConnection.start();
      this.connectionStateSignal.set(CanvasConnectionState.Connected);
      console.log('Canvas SignalR connected');
    } catch (error) {
      console.error('Canvas SignalR connection failed:', error);
      this.connectionStateSignal.set(CanvasConnectionState.Disconnected);
      throw error;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.currentCanvasId) {
      await this.leaveCanvas(this.currentCanvasId);
    }

    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this.connectionStateSignal.set(CanvasConnectionState.Disconnected);
      this.onlineUsersSignal.set([]);
      console.log('Canvas SignalR disconnected');
    }
  }

  async joinCanvas(canvasId: number, userName: string): Promise<void> {
    if (!this.hubConnection || this.connectionStateSignal() !== CanvasConnectionState.Connected) {
      await this.startConnection();
    }

    // Leave previous canvas if any
    if (this.currentCanvasId && this.currentCanvasId !== canvasId) {
      await this.leaveCanvas(this.currentCanvasId);
    }

    await this.hubConnection!.invoke('JoinCanvas', canvasId, userName);
    this.currentCanvasId = canvasId;
    console.log(`Joined canvas ${canvasId}`);
  }

  async leaveCanvas(canvasId: number): Promise<void> {
    if (!this.hubConnection) return;

    try {
      await this.hubConnection.invoke('LeaveCanvas', canvasId);
      if (this.currentCanvasId === canvasId) {
        this.currentCanvasId = null;
        this.onlineUsersSignal.set([]);
      }
      console.log(`Left canvas ${canvasId}`);
    } catch (error) {
      console.error('Error leaving canvas:', error);
    }
  }

  async broadcastSceneUpdate(
    canvasId: number,
    elements: string,
    appState: string | null
  ): Promise<void> {
    if (!this.hubConnection || this.connectionStateSignal() !== CanvasConnectionState.Connected) {
      return;
    }

    await this.hubConnection.invoke('BroadcastSceneUpdate', canvasId, elements, appState);
  }

  async broadcastPointerUpdate(
    canvasId: number,
    x: number,
    y: number,
    selectedElementId: string | null
  ): Promise<void> {
    if (!this.hubConnection || this.connectionStateSignal() !== CanvasConnectionState.Connected) {
      return;
    }

    await this.hubConnection.invoke('BroadcastPointerUpdate', canvasId, x, y, selectedElementId);
  }

  async broadcastSelection(canvasId: number, elementIds: string[]): Promise<void> {
    if (!this.hubConnection || this.connectionStateSignal() !== CanvasConnectionState.Connected) {
      return;
    }

    await this.hubConnection.invoke('BroadcastSelection', canvasId, elementIds);
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Scene updates from other users
    this.hubConnection.on('SceneUpdated', (event: SceneUpdateEvent) => {
      this.sceneUpdatedSubject.next(event);
    });

    // Pointer updates from other users
    this.hubConnection.on('PointerUpdated', (event: PointerUpdateEvent) => {
      this.pointerUpdatedSubject.next(event);
    });

    // User joined canvas
    this.hubConnection.on('UserJoined', (user: CanvasUser) => {
      this.onlineUsersSignal.update((users) => [...users, user]);
      this.userJoinedSubject.next(user);
    });

    // User left canvas
    this.hubConnection.on('UserLeft', (userId: string) => {
      this.onlineUsersSignal.update((users) =>
        users.filter((u) => u.userId !== userId)
      );
      this.userLeftSubject.next(userId);
    });

    // Initial users list when joining
    this.hubConnection.on('UsersInCanvas', (users: CanvasUser[]) => {
      this.onlineUsersSignal.set(users);
    });

    // Canvas updated from server (after save)
    this.hubConnection.on('CanvasUpdated', (event: CanvasUpdatedEvent) => {
      this.canvasUpdatedSubject.next(event);
    });

    // Canvas deleted
    this.hubConnection.on('CanvasDeleted', (event: CanvasDeletedEvent) => {
      this.canvasDeletedSubject.next(event);
    });

    // Canvas saved notification
    this.hubConnection.on('CanvasSaved', (data: { canvasId: number; version: number }) => {
      this.canvasSavedSubject.next(data);
    });
  }

  private setupConnectionHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting(() => {
      this.connectionStateSignal.set(CanvasConnectionState.Reconnecting);
      console.log('Canvas SignalR reconnecting...');
    });

    this.hubConnection.onreconnected(async () => {
      this.connectionStateSignal.set(CanvasConnectionState.Connected);
      console.log('Canvas SignalR reconnected');

      // Rejoin canvas if we were in one
      if (this.currentCanvasId) {
        const user = this.authService.user();
        const userName = user?.fullName ?? 'Unknown';
        await this.joinCanvas(this.currentCanvasId, userName);
      }
    });

    this.hubConnection.onclose(() => {
      this.connectionStateSignal.set(CanvasConnectionState.Disconnected);
      this.onlineUsersSignal.set([]);
      console.log('Canvas SignalR connection closed');
    });
  }
}
