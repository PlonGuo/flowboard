import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { BoardService } from '../../../core/services/board.service';
import { Column, Task } from '../../../core/models';

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './kanban-board.component.html',
  styleUrl: './kanban-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KanbanBoardComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly boardService = inject(BoardService);

  private readonly boardId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))));

  readonly board = computed(() => {
    const id = this.boardId();
    if (id) {
      return this.boardService.getBoard(id);
    }
    return undefined;
  });

  readonly selectedTask = signal<Task | undefined>(undefined);

  getTasksForColumn(column: Column): Task[] {
    const currentBoard = this.board();
    if (!currentBoard) return [];
    return column.tasks
      .map((taskId) => this.boardService.getTask(currentBoard, taskId))
      .filter((t): t is Task => t !== undefined);
  }

  openTaskDetails(task: Task): void {
    this.selectedTask.set(task);
  }

  closeTaskDetails(): void {
    this.selectedTask.set(undefined);
  }

  getChecklistProgress(task: Task): number {
    if (!task.checklist || task.checklist.length === 0) return 0;
    const completedItems = task.checklist.filter((item) => item.completed).length;
    return Math.round((completedItems / task.checklist.length) * 100);
  }
}
