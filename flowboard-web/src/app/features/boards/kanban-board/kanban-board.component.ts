import { ChangeDetectionStrategy, Component, inject, signal, OnInit, OnDestroy, computed, HostListener, ElementRef } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil, switchMap, filter } from 'rxjs';
import { CdkDragDrop, CdkDrag, CdkDropList, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { BoardService } from '../../../core/services/board.service';
import { TaskService } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { BoardDetailDto, ColumnApiDto, UserSummaryDto } from '../../../core/models/board.model';
import { TaskApiDto, TaskItemDto, MoveTaskRequest, CommentDto } from '../../../core/models/task.model';
import {
  TaskMovedEvent,
  CommentAddedEvent,
  CommentUpdatedEvent,
  CommentDeletedEvent,
} from '../../../core/models/signalr-events.model';
import { CommentService } from '../../../core/services/comment.service';
import { TaskFormDialogComponent } from '../components/task-form-dialog/task-form-dialog.component';
import { CommentListComponent } from '../components/comment-list/comment-list.component';
import { NotificationBellComponent } from '../../../shared/components/notification-bell/notification-bell.component';

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [RouterModule, TaskFormDialogComponent, CommentListComponent, CdkDropList, CdkDrag, NotificationBellComponent],
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KanbanBoardComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly boardService = inject(BoardService);
  private readonly taskService = inject(TaskService);
  private readonly commentService = inject(CommentService);
  private readonly authService = inject(AuthService);
  private readonly signalRService = inject(SignalRService);
  private readonly elementRef = inject(ElementRef);
  private readonly destroy$ = new Subject<void>();

  // SignalR connection state
  readonly connectionState = this.signalRService.connectionState;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    // Close dropdown if clicking outside of it
    if (this.dropdownOpen()) {
      const dropdownElement = this.elementRef.nativeElement.querySelector('.dropdown-container');
      if (dropdownElement && !dropdownElement.contains(event.target as Node)) {
        this.closeDropdown();
      }
    }
  }

  readonly board = signal<BoardDetailDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly selectedTask = signal<TaskApiDto | null>(null);

  // User state
  readonly user = this.authService.user;
  readonly avatarUrl = this.authService.avatarUrl;
  readonly dropdownOpen = signal(false);

  // Task form dialog state
  readonly showTaskForm = signal(false);
  readonly taskFormColumnId = signal<number | null>(null);
  readonly editingTask = signal<TaskItemDto | null>(null);
  readonly isDeleting = signal(false);
  readonly showDeleteConfirm = signal(false);
  readonly isLoadingTaskDetails = signal(false);

  // Drag and drop state
  readonly isMovingTask = signal(false);
  readonly moveError = signal<string | null>(null);

  // Comments state
  readonly taskComments = signal<CommentDto[]>([]);
  readonly isLoadingComments = signal(false);
  readonly isSavingComment = signal(false);

  // Board members state (for @mentions)
  readonly boardMembers = signal<UserSummaryDto[]>([]);

  // Computed list of column IDs for cdkDropListConnectedTo
  readonly columnDropListIds = computed(() => {
    const currentBoard = this.board();
    if (!currentBoard) return [];
    return currentBoard.columns.map((col) => `column-${col.id}`);
  });

  ngOnInit(): void {
    // Start SignalR connection
    this.signalRService.startConnection().catch((err) =>
      console.error('Failed to start SignalR connection:', err)
    );

    // Subscribe to real-time events
    this.setupSignalRSubscriptions();

    this.route.paramMap
      .pipe(
        takeUntil(this.destroy$),
        filter((params) => params.has('id')),
        switchMap((params) => {
          const id = params.get('id');
          this.loading.set(true);
          this.error.set(null);
          return this.boardService.getBoard(Number(id));
        })
      )
      .subscribe({
        next: (board) => {
          this.board.set(board);
          this.loading.set(false);

          // Join board group for real-time updates
          this.signalRService.joinBoard(board.id).catch((err) =>
            console.error('Failed to join board:', err)
          );

          // Load board members for @mentions
          this.loadBoardMembers(board.id);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load board');
          this.loading.set(false);
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    // Leave board group when leaving the page
    const currentBoard = this.board();
    if (currentBoard) {
      this.signalRService.leaveBoard(currentBoard.id).catch((err) =>
        console.error('Failed to leave board:', err)
      );
    }
  }

  getTasksForColumn(column: ColumnApiDto): TaskApiDto[] {
    return column.tasks || [];
  }

  openTaskDetails(task: TaskApiDto): void {
    this.selectedTask.set(task);
    this.loadTaskComments(task.id);
  }

  closeTaskDetails(): void {
    this.selectedTask.set(null);
    this.showDeleteConfirm.set(false);
    this.taskComments.set([]);
  }

  private loadTaskComments(taskId: number): void {
    this.isLoadingComments.set(true);
    this.taskService.getTask(taskId, true).subscribe({
      next: (taskDetail) => {
        this.taskComments.set(taskDetail.comments || []);
        this.isLoadingComments.set(false);
      },
      error: (err) => {
        console.error('Failed to load comments:', err);
        this.isLoadingComments.set(false);
      },
    });
  }

  private loadBoardMembers(boardId: number): void {
    this.boardService.getBoardMembers(boardId).subscribe({
      next: (members) => {
        this.boardMembers.set(members);
      },
      error: (err) => {
        console.error('Failed to load board members:', err);
      },
    });
  }

  getColumnColor(index: number): string {
    const colors = ['white/40', 'primary', 'green-400'];
    return colors[index % colors.length];
  }

  // User Dropdown Methods
  toggleDropdown(): void {
    this.dropdownOpen.update((v) => !v);
  }

  closeDropdown(): void {
    this.dropdownOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
  }

  // Task Form Dialog Methods
  openCreateTaskForm(columnId: number): void {
    this.taskFormColumnId.set(columnId);
    this.editingTask.set(null);
    this.showTaskForm.set(true);
  }

  openEditTaskForm(task: TaskApiDto): void {
    // Fetch full task details from API to get rowVersion for concurrency control
    this.isLoadingTaskDetails.set(true);
    this.taskService.getTask(task.id, false).subscribe({
      next: (taskDetail) => {
        const taskItemDto: TaskItemDto = {
          id: taskDetail.id,
          title: taskDetail.title,
          description: taskDetail.description,
          columnId: taskDetail.columnId,
          columnName: taskDetail.columnName,
          position: taskDetail.position,
          assignee: taskDetail.assignee,
          priority: taskDetail.priority,
          dueDate: taskDetail.dueDate,
          createdBy: taskDetail.createdBy,
          commentCount: taskDetail.comments?.length || 0,
          rowVersion: taskDetail.rowVersion,
          createdAt: taskDetail.createdAt,
          updatedAt: taskDetail.updatedAt,
        };
        this.editingTask.set(taskItemDto);
        this.taskFormColumnId.set(null);
        this.showTaskForm.set(true);
        this.isLoadingTaskDetails.set(false);
        this.closeTaskDetails();
      },
      error: (err) => {
        this.isLoadingTaskDetails.set(false);
        this.error.set(err.message || 'Failed to load task details');
      },
    });
  }

  closeTaskForm(): void {
    this.showTaskForm.set(false);
    this.taskFormColumnId.set(null);
    this.editingTask.set(null);
  }

  openTaskCanvas(task: TaskApiDto): void {
    const currentBoard = this.board();
    if (currentBoard) {
      this.router.navigate(['/board', currentBoard.id, 'task', task.id, 'canvas']);
    }
  }

  onTaskSaved(task: TaskItemDto): void {
    this.closeTaskForm();
    // Reload the board to refresh tasks
    this.reloadBoard();
  }

  // Delete Task Methods
  confirmDeleteTask(): void {
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
  }

  deleteTask(): void {
    const task = this.selectedTask();
    if (!task) return;

    this.isDeleting.set(true);
    this.taskService.deleteTask(task.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.closeTaskDetails();
        this.reloadBoard();
      },
      error: (err) => {
        this.isDeleting.set(false);
        this.error.set(err.message || 'Failed to delete task');
      },
    });
  }

  // Comment Methods
  onAddComment(content: string): void {
    const task = this.selectedTask();
    if (!task) return;

    this.isSavingComment.set(true);
    this.commentService.createComment(task.id, content).subscribe({
      next: (comment) => {
        this.taskComments.update((comments) => [...comments, comment]);
        this.isSavingComment.set(false);
        // Update the comment count in the task card
        this.updateTaskCommentCount(task.id, 1);
      },
      error: (err) => {
        this.isSavingComment.set(false);
        this.error.set(err.message || 'Failed to add comment');
      },
    });
  }

  onUpdateComment(event: { commentId: number; content: string }): void {
    this.commentService.updateComment(event.commentId, event.content).subscribe({
      next: (updatedComment) => {
        this.taskComments.update((comments) =>
          comments.map((c) => (c.id === updatedComment.id ? updatedComment : c))
        );
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to update comment');
      },
    });
  }

  onDeleteComment(commentId: number): void {
    const task = this.selectedTask();
    this.commentService.deleteComment(commentId).subscribe({
      next: () => {
        this.taskComments.update((comments) =>
          comments.filter((c) => c.id !== commentId)
        );
        // Update the comment count in the task card
        if (task) {
          this.updateTaskCommentCount(task.id, -1);
        }
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to delete comment');
      },
    });
  }

  private updateTaskCommentCount(taskId: number, delta: number): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    const updatedBoard = this.cloneBoard(currentBoard);
    for (const column of updatedBoard.columns) {
      const task = column.tasks?.find((t) => t.id === taskId);
      if (task) {
        task.commentsCount = (task.commentsCount || 0) + delta;
        this.board.set(updatedBoard);
        return;
      }
    }
  }

  private reloadBoard(): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    this.boardService.getBoard(currentBoard.id).subscribe({
      next: (board) => {
        this.board.set(board);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to reload board');
      },
    });
  }

  // Drag and Drop Methods
  onTaskDrop(event: CdkDragDrop<TaskApiDto[], TaskApiDto[], TaskApiDto>): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    // Get source and target column info from container data
    const sourceColumnId = event.previousContainer.data[0]
      ? this.getColumnIdFromTask(event.previousContainer.data, event.previousIndex)
      : null;
    const targetColumnId = this.getColumnIdFromContainerId(event.container.id);
    const task = event.item.data;

    if (!targetColumnId || !task) return;

    // Same position, no change needed
    if (
      event.previousContainer === event.container &&
      event.previousIndex === event.currentIndex
    ) {
      return;
    }

    // Optimistic UI update - update local state immediately
    const boardCopy = this.cloneBoard(currentBoard);
    const sourceColumn = boardCopy.columns.find((c) =>
      c.tasks?.some((t) => t.id === task.id)
    );
    const targetColumn = boardCopy.columns.find((c) => c.id === targetColumnId);

    if (!sourceColumn || !targetColumn) return;

    if (event.previousContainer === event.container) {
      // Reorder within same column
      moveItemInArray(
        targetColumn.tasks || [],
        event.previousIndex,
        event.currentIndex
      );
    } else {
      // Move to different column
      transferArrayItem(
        sourceColumn.tasks || [],
        targetColumn.tasks || [],
        event.previousIndex,
        event.currentIndex
      );
    }

    // Update UI immediately
    this.board.set(boardCopy);
    this.isMovingTask.set(true);
    this.moveError.set(null);

    // Fetch task details to get rowVersion, then call API
    this.taskService.getTask(task.id, false).subscribe({
      next: (taskDetail) => {
        const request: MoveTaskRequest = {
          toColumnId: targetColumnId,
          toPosition: event.currentIndex,
          rowVersion: taskDetail.rowVersion,
        };

        this.taskService.moveTask(task.id, request).subscribe({
          next: () => {
            this.isMovingTask.set(false);
            // Reload to ensure positions are synced
            this.reloadBoard();
          },
          error: (err) => {
            this.isMovingTask.set(false);
            this.moveError.set(err.message || 'Failed to move task');
            // Revert to original state on error
            this.board.set(currentBoard);
          },
        });
      },
      error: (err) => {
        this.isMovingTask.set(false);
        this.moveError.set(err.message || 'Failed to get task details');
        // Revert to original state on error
        this.board.set(currentBoard);
      },
    });
  }

  private getColumnIdFromContainerId(containerId: string): number | null {
    const match = containerId.match(/column-(\d+)/);
    return match ? parseInt(match[1], 10) : null;
  }

  private getColumnIdFromTask(tasks: TaskApiDto[], index: number): number | null {
    const currentBoard = this.board();
    if (!currentBoard || !tasks[index]) return null;

    const taskId = tasks[index].id;
    for (const column of currentBoard.columns) {
      if (column.tasks?.some((t) => t.id === taskId)) {
        return column.id;
      }
    }
    return null;
  }

  private cloneBoard(board: BoardDetailDto): BoardDetailDto {
    return {
      ...board,
      columns: board.columns.map((col) => ({
        ...col,
        tasks: col.tasks ? [...col.tasks] : [],
      })),
    };
  }

  // SignalR real-time event subscriptions
  private setupSignalRSubscriptions(): void {
    this.signalRService.onTaskCreated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((task) => this.handleTaskCreated(task));

    this.signalRService.onTaskUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((task) => this.handleTaskUpdated(task));

    this.signalRService.onTaskMoved$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => this.handleTaskMoved(event));

    this.signalRService.onTaskDeleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe((taskId) => this.handleTaskDeleted(taskId));

    // Comment events
    this.signalRService.onCommentAdded$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => this.handleCommentAdded(event));

    this.signalRService.onCommentUpdated$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => this.handleCommentUpdated(event));

    this.signalRService.onCommentDeleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event) => this.handleCommentDeleted(event));
  }

  private handleTaskCreated(task: TaskItemDto): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    const updatedBoard = this.cloneBoard(currentBoard);
    const column = updatedBoard.columns.find((c) => c.id === task.columnId);

    if (column) {
      // Map TaskItemDto to TaskApiDto format
      const taskApiDto: TaskApiDto = this.mapTaskItemDtoToTaskApiDto(task);
      column.tasks = [...(column.tasks || []), taskApiDto];
      // Sort by position
      column.tasks.sort((a, b) => a.position - b.position);
      this.board.set(updatedBoard);
      console.log('Real-time: Task created', task.id);
    }
  }

  private handleTaskUpdated(task: TaskItemDto): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    const updatedBoard = this.cloneBoard(currentBoard);

    for (const column of updatedBoard.columns) {
      const taskIndex = column.tasks?.findIndex((t) => t.id === task.id) ?? -1;
      if (taskIndex !== -1) {
        const taskApiDto = this.mapTaskItemDtoToTaskApiDto(task);
        column.tasks![taskIndex] = taskApiDto;
        this.board.set(updatedBoard);
        console.log('Real-time: Task updated', task.id);
        return;
      }
    }
  }

  private handleTaskMoved(event: TaskMovedEvent): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    const updatedBoard = this.cloneBoard(currentBoard);
    const sourceColumn = updatedBoard.columns.find(
      (c) => c.id === event.fromColumnId
    );
    const targetColumn = updatedBoard.columns.find(
      (c) => c.id === event.toColumnId
    );

    if (!sourceColumn || !targetColumn) return;

    const taskIndex =
      sourceColumn.tasks?.findIndex((t) => t.id === event.taskId) ?? -1;
    if (taskIndex === -1) return;

    // Remove from source
    const [movedTask] = sourceColumn.tasks!.splice(taskIndex, 1);
    movedTask.position = event.newPosition;

    // Add to target at the specified position
    targetColumn.tasks = targetColumn.tasks || [];
    targetColumn.tasks.splice(event.newPosition, 0, movedTask);

    // Reindex positions in affected columns
    targetColumn.tasks.forEach((t, i) => (t.position = i));
    if (sourceColumn !== targetColumn) {
      sourceColumn.tasks?.forEach((t, i) => (t.position = i));
    }

    this.board.set(updatedBoard);
    console.log('Real-time: Task moved', event.taskId);
  }

  private handleTaskDeleted(taskId: number): void {
    const currentBoard = this.board();
    if (!currentBoard) return;

    const updatedBoard = this.cloneBoard(currentBoard);

    for (const column of updatedBoard.columns) {
      const taskIndex = column.tasks?.findIndex((t) => t.id === taskId) ?? -1;
      if (taskIndex !== -1) {
        column.tasks!.splice(taskIndex, 1);
        // Reindex positions
        column.tasks?.forEach((t, i) => (t.position = i));
        this.board.set(updatedBoard);
        console.log('Real-time: Task deleted', taskId);
        return;
      }
    }
  }

  private mapTaskItemDtoToTaskApiDto(task: TaskItemDto): TaskApiDto {
    return {
      id: task.id,
      title: task.title,
      description: task.description,
      position: task.position,
      priority: task.priority,
      dueDate: task.dueDate,
      assignee: task.assignee
        ? {
            id: task.assignee.id,
            fullName: task.assignee.fullName,
            avatarUrl: task.assignee.avatarUrl,
          }
        : undefined,
      commentsCount: task.commentCount,
      attachmentsCount: 0,
      createdAt: task.createdAt,
      updatedAt: task.updatedAt,
    };
  }

  // Real-time comment event handlers
  private handleCommentAdded(event: CommentAddedEvent): void {
    const selectedTask = this.selectedTask();
    // Only update if viewing the same task
    if (selectedTask && selectedTask.id === event.taskId) {
      this.taskComments.update((comments) => [...comments, event.comment]);
    }
    // Update comment count in board view
    this.updateTaskCommentCount(event.taskId, 1);
    console.log('Real-time: Comment added to task', event.taskId);
  }

  private handleCommentUpdated(event: CommentUpdatedEvent): void {
    const selectedTask = this.selectedTask();
    // Only update if viewing the same task
    if (selectedTask && selectedTask.id === event.taskId) {
      this.taskComments.update((comments) =>
        comments.map((c) => (c.id === event.comment.id ? event.comment : c))
      );
    }
    console.log('Real-time: Comment updated on task', event.taskId);
  }

  private handleCommentDeleted(event: CommentDeletedEvent): void {
    const selectedTask = this.selectedTask();
    // Only update if viewing the same task
    if (selectedTask && selectedTask.id === event.taskId) {
      this.taskComments.update((comments) =>
        comments.filter((c) => c.id !== event.commentId)
      );
    }
    // Update comment count in board view
    this.updateTaskCommentCount(event.taskId, -1);
    console.log('Real-time: Comment deleted from task', event.taskId);
  }
}
