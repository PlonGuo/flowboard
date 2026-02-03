import { ChangeDetectionStrategy, Component, inject, signal, computed, OnInit, HostListener, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BoardService } from '../../../core/services/board.service';
import { AuthService } from '../../../core/services/auth.service';
import { TeamService } from '../../../core/services/team.service';
import { BoardFormDialogComponent } from '../components/board-form-dialog/board-form-dialog.component';
import { JoinTeamDialogComponent } from '../../teams/join-team-dialog/join-team-dialog.component';
import { CreateTeamDialogComponent } from '../../teams/create-team-dialog/create-team-dialog.component';
import { BoardSummaryDto } from '../../../core/models/board.model';
import { TeamDto, TeamRole } from '../../../core/models/team.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, BoardFormDialogComponent, JoinTeamDialogComponent, CreateTeamDialogComponent],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private readonly boardService = inject(BoardService);
  private readonly authService = inject(AuthService);
  private readonly teamService = inject(TeamService);
  private readonly elementRef = inject(ElementRef);

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

  // Board state from service
  readonly boards = this.boardService.boards;
  readonly loading = this.boardService.loading;
  readonly error = this.boardService.error;

  // Team state from service
  readonly teams = this.teamService.teams;
  readonly teamsLoading = this.teamService.loading;

  // Invite code copy state
  readonly copiedTeamId = signal<number | null>(null);

  // User state
  readonly user = this.authService.user;
  readonly avatarUrl = this.authService.avatarUrl;

  // UI state
  readonly dropdownOpen = signal(false);
  readonly showCreateDialog = signal(false);
  readonly showJoinTeamDialog = signal(false);
  readonly showCreateTeamDialog = signal(false);
  readonly showDeleteConfirm = signal(false);
  readonly boardToDelete = signal<BoardSummaryDto | null>(null);
  readonly isDeleting = signal(false);

  // Team management state
  readonly showTeamDeleteConfirm = signal(false);
  readonly teamToDelete = signal<TeamDto | null>(null);
  readonly isDeletingTeam = signal(false);
  readonly selectedTeamFilter = signal<number | null>(null); // null = All Teams

  // Computed filtered boards based on team selection
  readonly filteredBoards = computed(() => {
    const selectedTeam = this.selectedTeamFilter();
    const allBoards = this.boards();

    if (selectedTeam === null) {
      return allBoards; // Show all boards
    }

    return allBoards.filter((board) => board.teamId === selectedTeam);
  });

  ngOnInit(): void {
    this.loadBoards();
    this.loadTeams();
  }

  loadBoards(): void {
    this.boardService.loadUserBoards().subscribe();
  }

  loadTeams(): void {
    this.teamService.loadUserTeams().subscribe();
  }

  copyInviteCode(team: TeamDto): void {
    if (!team.inviteCode) return;

    navigator.clipboard.writeText(team.inviteCode).then(() => {
      this.copiedTeamId.set(team.id);
      setTimeout(() => this.copiedTeamId.set(null), 2000);
    });
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

  openJoinTeamDialog(): void {
    this.showJoinTeamDialog.set(true);
  }

  closeJoinTeamDialog(): void {
    this.showJoinTeamDialog.set(false);
  }

  onTeamJoined(): void {
    // Reload boards and teams to show boards from the newly joined team
    this.loadBoards();
    this.loadTeams();
  }

  openCreateTeamDialog(): void {
    this.showCreateTeamDialog.set(true);
  }

  closeCreateTeamDialog(): void {
    this.showCreateTeamDialog.set(false);
  }

  onTeamCreated(): void {
    // Team is already added to the list by the service
    // Just close the dialog
    this.closeCreateTeamDialog();
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

  // Team filtering
  selectTeamFilter(teamId: number | null): void {
    this.selectedTeamFilter.set(teamId);
  }

  // Team delete/leave methods
  confirmTeamAction(team: TeamDto, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.teamToDelete.set(team);
    this.showTeamDeleteConfirm.set(true);
  }

  cancelTeamAction(): void {
    this.showTeamDeleteConfirm.set(false);
    this.teamToDelete.set(null);
  }

  executeTeamAction(): void {
    const team = this.teamToDelete();
    if (!team) return;

    this.isDeletingTeam.set(true);

    const isOwner = this.isTeamOwner(team);
    const action$ = isOwner
      ? this.teamService.deleteTeam(team.id)
      : this.teamService.leaveTeam(team.id);

    action$.subscribe({
      next: () => {
        this.isDeletingTeam.set(false);
        this.cancelTeamAction();
        // Reset filter if the deleted team was selected
        if (this.selectedTeamFilter() === team.id) {
          this.selectedTeamFilter.set(null);
        }
        // Reload boards since team deletion may cascade
        if (isOwner) {
          this.loadBoards();
        }
      },
      error: () => {
        this.isDeletingTeam.set(false);
      },
    });
  }

  isTeamOwner(team: TeamDto): boolean {
    return team.currentUserRole === TeamRole.Owner;
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
