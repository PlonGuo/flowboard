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
import { BoardService } from '../../../../core/services/board.service';
import { TeamService } from '../../../../core/services/team.service';
import { CreateBoardRequest, BoardDetailDto, BoardDto } from '../../../../core/models/board.model';
import { TeamDto } from '../../../../core/models/team.model';

@Component({
  selector: 'app-board-form-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './board-form-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BoardFormDialogComponent implements OnInit {
  @Input() board?: BoardDetailDto | BoardDto;
  @Output() saved = new EventEmitter<BoardDetailDto | BoardDto>();
  @Output() cancelled = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly boardService = inject(BoardService);
  private readonly teamService = inject(TeamService);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly teams = signal<TeamDto[]>([]);
  readonly loadingTeams = signal(true);
  readonly showTeamForm = signal(false);
  readonly isCreatingTeam = signal(false);

  readonly form: FormGroup<{
    name: FormControl<string>;
    description: FormControl<string>;
    teamId: FormControl<number>;
  }>;

  readonly teamForm: FormGroup<{
    name: FormControl<string>;
    description: FormControl<string>;
  }>;

  get isEditMode(): boolean {
    return !!this.board;
  }

  constructor() {
    this.form = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      teamId: [0, [Validators.required, Validators.min(1)]],
    });

    this.teamForm = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
    });
  }

  ngOnInit(): void {
    if (this.board) {
      this.form.patchValue({
        name: this.board.name,
        description: this.board.description || '',
      });
    }

    // Load available teams
    this.teamService.loadUserTeams().subscribe({
      next: (teams) => {
        this.teams.set(teams);
        this.loadingTeams.set(false);
        // Auto-select first team if available
        if (teams.length > 0) {
          this.form.patchValue({ teamId: teams[0].id });
        }
      },
      error: () => {
        this.loadingTeams.set(false);
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const formValue = this.form.getRawValue();

    if (this.board) {
      // Update mode
      this.boardService
        .updateBoard(this.board.id, {
          name: formValue.name.trim(),
          description: formValue.description.trim() || undefined,
        })
        .subscribe({
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
      const request: CreateBoardRequest = {
        name: formValue.name.trim(),
        description: formValue.description.trim() || undefined,
        teamId: formValue.teamId,
      };

      this.boardService.createBoard(request).subscribe({
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

  toggleTeamForm(): void {
    this.showTeamForm.update((v) => !v);
    if (!this.showTeamForm()) {
      this.teamForm.reset();
    }
  }

  onCreateTeam(): void {
    if (this.teamForm.invalid) {
      this.teamForm.markAllAsTouched();
      return;
    }

    this.isCreatingTeam.set(true);
    this.errorMessage.set(null);

    const formValue = this.teamForm.getRawValue();

    this.teamService
      .createTeam({
        name: formValue.name.trim(),
        description: formValue.description.trim() || undefined,
      })
      .subscribe({
        next: (team) => {
          this.isCreatingTeam.set(false);
          this.teams.update((teams) => [team, ...teams]);
          this.form.patchValue({ teamId: team.id });
          this.showTeamForm.set(false);
          this.teamForm.reset();
        },
        error: (err) => {
          this.isCreatingTeam.set(false);
          this.errorMessage.set(err.message);
        },
      });
  }

  onCancel(): void {
    this.cancelled.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }
}
