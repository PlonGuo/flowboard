import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  ElementRef,
  ViewChild,
  AfterViewInit,
  ChangeDetectionStrategy,
  NgZone,
} from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil, debounceTime } from 'rxjs';
import { CanvasService } from '../../../core/services/canvas.service';
import { CanvasSignalRService } from '../../../core/services/canvas-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { CanvasDetailDto, CanvasUser } from '../../../core/models/canvas.model';

@Component({
  selector: 'app-task-canvas',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './task-canvas.component.html',
  styleUrl: './task-canvas.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskCanvasComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('excalidrawContainer') excalidrawContainer!: ElementRef<HTMLDivElement>;

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly canvasService = inject(CanvasService);
  private readonly canvasSignalR = inject(CanvasSignalRService);
  private readonly authService = inject(AuthService);
  private readonly ngZone = inject(NgZone);
  private readonly destroy$ = new Subject<void>();

  // Route params
  readonly boardId = signal<number | null>(null);
  readonly taskId = signal<number | null>(null);

  // Canvas state
  readonly canvas = signal<CanvasDetailDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly lastSavedVersion = signal<number>(0);

  // Excalidraw instance
  private excalidrawApi: any = null;
  private saveSubject = new Subject<{ elements: string; appState: string | null }>();

  // Online users
  readonly onlineUsers = this.canvasSignalR.onlineUsers;

  ngOnInit(): void {
    // Parse route params
    const boardIdParam = this.route.snapshot.paramMap.get('boardId');
    const taskIdParam = this.route.snapshot.paramMap.get('taskId');

    if (boardIdParam) this.boardId.set(parseInt(boardIdParam, 10));
    if (taskIdParam) this.taskId.set(parseInt(taskIdParam, 10));

    // Setup auto-save with debounce
    this.saveSubject
      .pipe(takeUntil(this.destroy$), debounceTime(5000))
      .subscribe((data) => this.saveCanvas(data.elements, data.appState));

    // Subscribe to remote scene updates
    this.canvasSignalR.onSceneUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => {
        if (this.excalidrawApi) {
          this.ngZone.run(() => {
            try {
              const elements = JSON.parse(event.elements);
              this.excalidrawApi.updateScene({ elements });
            } catch (e) {
              console.error('Failed to apply remote scene update:', e);
            }
          });
        }
      });

    // Load canvas
    this.loadCanvas();
  }

  ngAfterViewInit(): void {
    // Excalidraw will be mounted after canvas data is loaded
  }

  ngOnDestroy(): void {
    // Save any pending changes
    if (this.excalidrawApi && this.canvas()) {
      const elements = this.excalidrawApi.getSceneElements();
      const appState = this.excalidrawApi.getAppState();
      this.saveCanvas(JSON.stringify(elements), JSON.stringify(appState));
    }

    // Leave canvas SignalR group
    const canvasData = this.canvas();
    if (canvasData) {
      this.canvasSignalR.leaveCanvas(canvasData.id);
    }

    this.destroy$.next();
    this.destroy$.complete();
  }

  private async loadCanvas(): Promise<void> {
    const taskId = this.taskId();
    if (!taskId) {
      this.error.set('Invalid task ID');
      this.loading.set(false);
      return;
    }

    this.canvasService.getTaskCanvas(taskId).subscribe({
      next: async (canvas) => {
        if (canvas) {
          this.canvas.set(canvas);
          this.lastSavedVersion.set(canvas.data?.version ?? 0);
          await this.initializeExcalidraw(canvas);
        } else {
          // No canvas exists, create one
          await this.createCanvas();
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load canvas');
        this.loading.set(false);
      },
    });
  }

  private async createCanvas(): Promise<void> {
    const taskId = this.taskId();
    if (!taskId) return;

    this.canvasService.createTaskCanvas(taskId, 'Task Canvas').subscribe({
      next: async (canvas) => {
        this.canvas.set(canvas);
        this.lastSavedVersion.set(canvas.data?.version ?? 0);
        await this.initializeExcalidraw(canvas);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to create canvas');
      },
    });
  }

  private async initializeExcalidraw(canvas: CanvasDetailDto): Promise<void> {
    // Dynamic import of Excalidraw and React
    const [{ Excalidraw }, React, ReactDOM] = await Promise.all([
      import('@excalidraw/excalidraw'),
      import('react'),
      import('react-dom/client'),
    ]);

    // Parse initial data
    let initialElements: any[] = [];
    let initialAppState: any = {};

    if (canvas.data?.elements) {
      try {
        initialElements = JSON.parse(canvas.data.elements);
      } catch (e) {
        console.error('Failed to parse canvas elements:', e);
      }
    }

    if (canvas.data?.appState) {
      try {
        initialAppState = JSON.parse(canvas.data.appState);
      } catch (e) {
        console.error('Failed to parse canvas appState:', e);
      }
    }

    // Join canvas SignalR group
    const user = this.authService.user();
    const userName = user?.fullName ?? 'Anonymous';
    await this.canvasSignalR.joinCanvas(canvas.id, userName);

    // Mount Excalidraw in Angular zone
    this.ngZone.runOutsideAngular(() => {
      const container = this.excalidrawContainer.nativeElement;
      const root = ReactDOM.createRoot(container);

      const excalidrawElement = React.createElement(Excalidraw, {
        initialData: {
          elements: initialElements,
          appState: {
            ...initialAppState,
            viewBackgroundColor: '#1a1a2e',
            theme: 'dark',
          },
        },
        onChange: (elements: readonly unknown[], appState: unknown) => {
          this.handleExcalidrawChange([...elements], appState);
        },
        onPointerUpdate: (payload: unknown) => {
          this.handlePointerUpdate(payload);
        },
        excalidrawAPI: (api: unknown) => {
          this.excalidrawApi = api;
        },
        UIOptions: {
          canvasActions: {
            changeViewBackgroundColor: true,
            clearCanvas: true,
            export: { saveFileToDisk: true },
            loadScene: true,
            saveToActiveFile: false,
            toggleTheme: true,
          },
        },
      });

      root.render(excalidrawElement);
    });
  }

  private handleExcalidrawChange(elements: unknown[], appState: unknown): void {
    const canvas = this.canvas();
    if (!canvas) return;

    const elementsJson = JSON.stringify(elements);
    const appStateJson = JSON.stringify(appState);

    // Queue for auto-save
    this.saveSubject.next({ elements: elementsJson, appState: appStateJson });

    // Broadcast to other users (throttled by SignalR)
    this.canvasSignalR.broadcastSceneUpdate(canvas.id, elementsJson, appStateJson);
  }

  private handlePointerUpdate(payload: unknown): void {
    const canvas = this.canvas();
    const pointerPayload = payload as { pointer?: { x: number; y: number }; selectedElementIds?: Record<string, boolean> };
    if (!canvas || !pointerPayload.pointer) return;

    this.canvasSignalR.broadcastPointerUpdate(
      canvas.id,
      pointerPayload.pointer.x,
      pointerPayload.pointer.y,
      pointerPayload.selectedElementIds ? Object.keys(pointerPayload.selectedElementIds)[0] : null
    );
  }

  private saveCanvas(elements: string, appState: string | null): void {
    const canvas = this.canvas();
    if (!canvas) return;

    this.isSaving.set(true);
    this.canvasService
      .saveCanvasData(canvas.id, { elements, appState: appState ?? undefined })
      .subscribe({
        next: (response) => {
          this.isSaving.set(false);
          this.lastSavedVersion.set(response.version);
        },
        error: (err) => {
          this.isSaving.set(false);
          console.error('Failed to save canvas:', err);
        },
      });
  }

  navigateBack(): void {
    const boardId = this.boardId();
    if (boardId) {
      this.router.navigate(['/board', boardId]);
    } else {
      this.router.navigate(['/']);
    }
  }
}
