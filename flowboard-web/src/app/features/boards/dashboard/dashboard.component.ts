import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BoardService } from '../../../core/services/board.service';
import { AuthService } from '../../../core/services/auth.service';
import { BoardFormDialogComponent } from '../components/board-form-dialog/board-form-dialog.component';
import { BoardSummaryDto } from '../../../core/models/board.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, BoardFormDialogComponent],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private readonly boardService = inject(BoardService);
  private readonly authService = inject(AuthService);

  // Board state from service
  readonly boards = this.boardService.boards;
  readonly loading = this.boardService.loading;
  readonly error = this.boardService.error;

  // User state
  readonly user = this.authService.user;
  readonly avatarUrl = this.authService.avatarUrl;

  // UI state
  readonly dropdownOpen = signal(false);
  readonly showCreateDialog = signal(false);
  readonly showDeleteConfirm = signal(false);
  readonly boardToDelete = signal<BoardSummaryDto | null>(null);
  readonly isDeleting = signal(false);

  ngOnInit(): void {
    this.loadBoards();
  }

  loadBoards(): void {
    this.boardService.loadUserBoards().subscribe();
  }

  openCreateDialog(): void {
    this.showCreateDialog.set(true);
  }

  closeCreateDialog(): void {
    this.showCreateDialog.set(false);
  }

  onBoardCreated(): void {
    this.closeCreateDialog();
  }

  confirmDeleteBoard(board: BoardSummaryDto, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.boardToDelete.set(board);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.boardToDelete.set(null);
  }

  deleteBoard(): void {
    const board = this.boardToDelete();
    if (!board) return;

    this.isDeleting.set(true);
    this.boardService.deleteBoard(board.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.cancelDelete();
      },
      error: () => {
        this.isDeleting.set(false);
      },
    });
  }

  toggleDropdown(): void {
    this.dropdownOpen.update((v) => !v);
  }

  closeDropdown(): void {
    this.dropdownOpen.set(false);
  }

  logout(): void {
    this.authService.logout();
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffDays = Math.floor(diffHours / 24);

    if (diffHours < 1) return 'Just now';
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }
}
