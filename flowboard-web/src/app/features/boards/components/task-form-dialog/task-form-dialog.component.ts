import {
  Component,
  inject,
  signal,
  Output,
  EventEmitter,
  Input,
  ChangeDetectionStrategy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
  FormControl,
} from '@angular/forms';
import { TaskService } from '../../../../core/services/task.service';
import { BoardService } from '../../../../core/services/board.service';
import {
  TaskItemDto,
  TaskPriority,
  CreateTaskRequest,
  UpdateTaskRequest,
} from '../../../../core/models/task.model';
import { UserSummaryDto } from '../../../../core/models/board.model';

@Component({
  selector: 'app-task-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-form-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskFormDialogComponent implements OnInit {
  @Input() task?: TaskItemDto | null;
  @Input() columnId?: number | null;
  @Input() boardId?: number | null;
  @Output() saved = new EventEmitter<TaskItemDto>();
  @Output() cancelled = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);
  private readonly boardService = inject(BoardService);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly teamMembers = signal<UserSummaryDto[]>([]);
  readonly loadingMembers = signal(false);

  readonly priorities = [
    { value: TaskPriority.Low, label: 'Low', color: 'bg-blue-500' },
    { value: TaskPriority.Medium, label: 'Medium', color: 'bg-yellow-500' },
    { value: TaskPriority.High, label: 'High', color: 'bg-red-500' },
  ];

  readonly form: FormGroup<{
    title: FormControl<string>;
    description: FormControl<string>;
    priority: FormControl<string>;
    dueDate: FormControl<string>;
    assigneeId: FormControl<number | null>;
  }>;

  get isEditMode(): boolean {
    return !!this.task;
  }

  constructor() {
    this.form = this.fb.group({
      title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
      description: this.fb.nonNullable.control('', [Validators.maxLength(2000)]),
      priority: this.fb.nonNullable.control(TaskPriority.Medium as string),
      dueDate: this.fb.nonNullable.control(''),
      assigneeId: this.fb.control<number | null>(null),
    });
  }

  ngOnInit(): void {
    // Load team members for assignee dropdown
    if (this.boardId) {
      this.loadingMembers.set(true);
      this.boardService.getBoardMembers(this.boardId).subscribe({
        next: (members) => {
          this.teamMembers.set(members);
          this.loadingMembers.set(false);
        },
        error: () => {
          this.loadingMembers.set(false);
        },
      });
    }

    // Populate form when editing
    if (this.task) {
      this.form.patchValue({
        title: this.task.title,
        description: this.task.description || '',
        priority: this.getPriorityString(this.task.priority as unknown as string | number),
        dueDate: this.task.dueDate
          ? this.formatDateForInput(this.task.dueDate)
          : '',
        assigneeId: this.task.assignee?.id ?? null,
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const formValue = this.form.getRawValue();

    if (this.task) {
      // Update mode - ensure priority is sent as string
      const request: UpdateTaskRequest = {
        title: formValue.title.trim(),
        description: formValue.description.trim() || undefined,
        priority: this.getPriorityString(formValue.priority),
        assigneeId: formValue.assigneeId,
        dueDate: formValue.dueDate || null,
        rowVersion: this.task.rowVersion,
      };

      this.taskService.updateTask(this.task.id, request).subscribe({
        next: (result) => {
          this.isSubmitting.set(false);
          this.saved.emit(result);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(err.message);
        },
      });
    } else {
      // Create mode
      if (!this.columnId) {
        this.errorMessage.set('Column ID is required');
        this.isSubmitting.set(false);
        return;
      }

      const request: CreateTaskRequest = {
        title: formValue.title.trim(),
        description: formValue.description.trim() || undefined,
        columnId: this.columnId,
        priority: formValue.priority as TaskPriority,
        assigneeId: formValue.assigneeId ?? undefined,
        dueDate: formValue.dueDate ? new Date(formValue.dueDate) : undefined,
      };

      this.taskService.createTask(request).subscribe({
        next: (result) => {
          this.isSubmitting.set(false);
          this.saved.emit(result);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(err.message);
        },
      });
    }
  }

  onCancel(): void {
    this.cancelled.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }

  private formatDateForInput(dateString: string): string {
    const date = new Date(dateString);
    return date.toISOString().split('T')[0];
  }

  private getPriorityString(priority: string | number): string {
    // Backend may return priority as number (0=Low, 1=Medium, 2=High)
    // We need to send it as string
    const priorityMap: Record<number, string> = {
      0: 'Low',
      1: 'Medium',
      2: 'High',
    };
    if (typeof priority === 'number') {
      return priorityMap[priority] || 'Medium';
    }
    return priority;
  }
}
