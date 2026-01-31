import { ChangeDetectionStrategy, Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subject, takeUntil, switchMap, filter } from 'rxjs';
import { BoardService } from '../../../core/services/board.service';
import { BoardDetailDto, ColumnApiDto } from '../../../core/models/board.model';
import { TaskApiDto } from '../../../core/models/task.model';

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KanbanBoardComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly boardService = inject(BoardService);
  private readonly destroy$ = new Subject<void>();

  readonly board = signal<BoardDetailDto | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly selectedTask = signal<TaskApiDto | null>(null);

  ngOnInit(): void {
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
  }

  getTasksForColumn(column: ColumnApiDto): TaskApiDto[] {
    return column.tasks || [];
  }

  openTaskDetails(task: TaskApiDto): void {
    this.selectedTask.set(task);
  }

  closeTaskDetails(): void {
    this.selectedTask.set(null);
  }

  getColumnColor(index: number): string {
    const colors = ['white/40', 'primary', 'green-400'];
    return colors[index % colors.length];
  }
}
