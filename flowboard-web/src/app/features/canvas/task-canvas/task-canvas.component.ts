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
  WritableSignal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil, debounceTime, throttleTime } from 'rxjs';
import { CanvasService } from '../../../core/services/canvas.service';
import { CanvasSignalRService } from '../../../core/services/canvas-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { CanvasDetailDto, CanvasUser } from '../../../core/models/canvas.model';

/** Tool definition for the custom toolbar */
interface CanvasTool {
  type: string;
  icon: string;
  label: string;
}

/** Available drawing tools matching Excalidraw's tool types */
const CANVAS_TOOLS: CanvasTool[] = [
  { type: 'selection', icon: 'near_me', label: 'Select' },
  { type: 'rectangle', icon: 'rectangle', label: 'Rectangle' },
  { type: 'diamond', icon: 'change_history', label: 'Diamond' },
  { type: 'ellipse', icon: 'circle', label: 'Ellipse' },
  { type: 'arrow', icon: 'arrow_right_alt', label: 'Arrow' },
  { type: 'line', icon: 'horizontal_rule', label: 'Line' },
  { type: 'freedraw', icon: 'draw', label: 'Draw' },
  { type: 'text', icon: 'title', label: 'Text' },
  { type: 'image', icon: 'image', label: 'Image' },
  { type: 'eraser', icon: 'ink_eraser', label: 'Eraser' },
];

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
  private broadcastSubject = new Subject<{ canvasId: number; elements: string; appState: string }>();
  // Counter of pending remote updates — each updateScene() increments,
  // each onChange consumed in handleExcalidrawChange decrements.
  // This avoids timing issues with setTimeout/RAF vs React's render cycle.
  private pendingRemoteUpdates = 0;

  // Online users
  readonly onlineUsers = this.canvasSignalR.onlineUsers;

  // Custom toolbar state
  readonly tools: WritableSignal<CanvasTool[]> = signal(CANVAS_TOOLS);
  readonly activeTool = signal<string>('selection');

  /**
   * Select a drawing tool and update Excalidraw
   */
  selectTool(toolType: string): void {
    this.activeTool.set(toolType);

    if (this.excalidrawApi) {
      // Use Excalidraw's setActiveTool API
      this.excalidrawApi.setActiveTool({ type: toolType });
    }
  }

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

    // Throttle outgoing broadcasts to max ~10/sec to avoid flooding SignalR
    this.broadcastSubject
      .pipe(takeUntil(this.destroy$), throttleTime(100))
      .subscribe(({ canvasId, elements, appState }) => {
        console.log('[Canvas] Broadcasting scene update to others');
        this.canvasSignalR.broadcastSceneUpdate(canvasId, elements, appState);
      });

    // Subscribe to remote scene updates
    this.canvasSignalR.onSceneUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => {
        console.log('[Canvas] Received remote scene update from user:', event.userId);
        if (this.excalidrawApi) {
          this.ngZone.run(() => {
            try {
              // Increment counter — handleExcalidrawChange will consume it
              // when React fires onChange after this updateScene call
              this.pendingRemoteUpdates++;
              const elements = JSON.parse(event.elements);
              this.excalidrawApi.updateScene({ elements });
              // Safety: if React never fires onChange (e.g. identical scene),
              // decrement after 2s so the counter doesn't stay stuck
              setTimeout(() => {
                if (this.pendingRemoteUpdates > 0) {
                  this.pendingRemoteUpdates--;
                  console.log('[Canvas] Safety timeout: decremented pending counter');
                }
              }, 2000);
            } catch (e) {
              console.error('Failed to apply remote scene update:', e);
              if (this.pendingRemoteUpdates > 0) this.pendingRemoteUpdates--;
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
          // Set loading to false first so the container is rendered
          this.loading.set(false);
          // initializeExcalidraw handles waiting for container
          await this.initializeExcalidraw(canvas);
        } else {
          // No canvas exists, create one
          await this.createCanvas();
        }
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
        // Set loading to false first so the container is rendered
        this.loading.set(false);
        // initializeExcalidraw handles waiting for container
        await this.initializeExcalidraw(canvas);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to create canvas');
        this.loading.set(false);
      },
    });
  }

  /**
   * Waits for the excalidraw container to be available in the DOM.
   * Uses requestAnimationFrame to ensure Angular change detection has completed.
   * @returns Promise resolving to the container element
   * @throws Error if container is not found within timeout
   */
  private waitForContainer(): Promise<HTMLDivElement> {
    return new Promise((resolve, reject) => {
      let attempts = 0;
      const maxAttempts = 50; // ~800ms at 60fps

      const checkContainer = (): void => {
        attempts++;

        // Check if container is available
        if (this.excalidrawContainer?.nativeElement) {
          resolve(this.excalidrawContainer.nativeElement);
          return;
        }

        // Timeout after max attempts
        if (attempts >= maxAttempts) {
          reject(new Error('Excalidraw container not found after timeout'));
          return;
        }

        // Schedule next check on next animation frame
        requestAnimationFrame(checkContainer);
      };

      // Begin checking after next render cycle
      requestAnimationFrame(checkContainer);
    });
  }

  private async initializeExcalidraw(canvas: CanvasDetailDto): Promise<void> {
    // Wait for container with proper error handling
    let container: HTMLDivElement;
    try {
      container = await this.waitForContainer();
    } catch (containerError) {
      console.error('Failed to initialize Excalidraw container:', containerError);
      this.error.set('Failed to initialize canvas. Please refresh the page.');
      return;
    }

    // Verify container dimensions (Excalidraw needs a sized container)
    if (container.offsetWidth === 0 || container.offsetHeight === 0) {
      console.warn('Excalidraw container has zero dimensions, waiting for layout...');
      await new Promise(resolve => requestAnimationFrame(resolve));
    }

    // Dynamic import of Excalidraw and React
    let Excalidraw: unknown;
    let React: typeof import('react');
    let ReactDOM: typeof import('react-dom/client');

    try {
      [{ Excalidraw }, React, ReactDOM] = await Promise.all([
        import('@excalidraw/excalidraw'),
        import('react'),
        import('react-dom/client'),
      ]);
    } catch (importError) {
      console.error('Failed to load Excalidraw dependencies:', importError);
      this.error.set('Failed to load canvas library. Please check your connection and refresh.');
      return;
    }

    // Parse initial data
    let initialElements: unknown[] = [];
    let initialAppState: Record<string, unknown> = {};

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

    // Mount Excalidraw outside Angular zone for performance
    this.ngZone.runOutsideAngular(() => {
      const root = ReactDOM.createRoot(container);

      // Remove collaborators from saved appState as it can't be serialized properly
      // Excalidraw expects collaborators to be a Map, not a plain object
      const { collaborators: _ignored, ...cleanAppState } = initialAppState;

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const excalidrawElement = React.createElement(Excalidraw as any, {
        initialData: {
          elements: initialElements,
          appState: {
            ...cleanAppState,
            collaborators: new Map(),
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
            changeViewBackgroundColor: false,
            clearCanvas: true,
            export: { saveFileToDisk: true },
            loadScene: false,
            saveToActiveFile: false,
            toggleTheme: false,
          },
          // Hide the default toolbar - we use our custom toolbar
          tools: {
            image: false, // We handle this in our toolbar
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

    // Queue for auto-save (always save, even for remote updates)
    this.saveSubject.next({ elements: elementsJson, appState: appStateJson });

    // Only broadcast local changes — if there are pending remote updates,
    // this onChange is an echo from updateScene, so consume the counter and skip
    if (this.pendingRemoteUpdates > 0) {
      this.pendingRemoteUpdates--;
      console.log('[Canvas] Skipping broadcast — consumed remote update echo, remaining:', this.pendingRemoteUpdates);
      return;
    }
    this.broadcastSubject.next({ canvasId: canvas.id, elements: elementsJson, appState: appStateJson });
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

  /**
   * Navigate back to the board or use browser history.
   * Provides graceful fallback when boardId is unavailable.
   */
  navigateBack(): void {
    const boardId = this.boardId();

    // Prefer explicit board navigation if we have a valid boardId
    if (boardId !== null && !Number.isNaN(boardId) && boardId > 0) {
      this.router.navigate(['/board', boardId]);
      return;
    }

    // Fallback: use browser history if available
    if (window.history.length > 1) {
      window.history.back();
      return;
    }

    // Last resort: go to dashboard
    this.router.navigate(['/']);
  }
}
