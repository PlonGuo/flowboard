import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TeamService } from '../../../core/services/team.service';
import { TeamDto } from '../../../core/models/team.model';

@Component({
  selector: 'app-join-team-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div
        class="absolute inset-0 bg-black/60 backdrop-blur-sm"
        (click)="onCancel()"
      ></div>
      <div
        class="glass-card relative w-full max-w-md p-8 rounded-2xl shadow-2xl bg-gray-900/95 border border-white/10"
      >
        <!-- Header -->
        <div class="text-center mb-6">
          <div
            class="size-16 rounded-full bg-primary/20 flex items-center justify-center mx-auto mb-4"
          >
            <span class="material-symbols-outlined text-4xl text-primary"
              >group_add</span
            >
          </div>
          <h2 class="text-2xl font-bold text-white">Join a Team</h2>
          <p class="text-white/50 mt-2">
            Enter the invite code to join an existing team
          </p>
        </div>

        <!-- Error Message -->
        @if (error()) {
          <div
            class="mb-4 p-3 bg-red-500/20 border border-red-500/40 rounded-lg text-red-400 text-sm flex items-center gap-2"
          >
            <span class="material-symbols-outlined text-[18px]">error</span>
            <span>{{ error() }}</span>
          </div>
        }

        <!-- Success Message -->
        @if (joinedTeam()) {
          <div class="text-center py-6">
            <div
              class="size-16 rounded-full bg-green-500/20 flex items-center justify-center mx-auto mb-4"
            >
              <span class="material-symbols-outlined text-4xl text-green-400"
                >check_circle</span
              >
            </div>
            <h3 class="text-xl font-bold text-white mb-2">Welcome!</h3>
            <p class="text-white/60 mb-1">
              You've joined <span class="text-primary font-semibold">{{ joinedTeam()?.name }}</span>
            </p>
            <p class="text-white/40 text-sm">
              {{ joinedTeam()?.memberCount }} members
            </p>
            <button
              (click)="onCancel()"
              class="mt-6 px-8 py-3 bg-primary hover:bg-primary/90 rounded-full font-bold text-white transition-colors"
            >
              Get Started
            </button>
          </div>
        } @else {
          <!-- Form -->
          <form (ngSubmit)="onSubmit()">
            <div class="mb-6">
              <label
                for="inviteCode"
                class="block text-sm font-semibold text-white/80 mb-2"
                >Invite Code</label
              >
              <input
                id="inviteCode"
                type="text"
                [(ngModel)]="inviteCode"
                name="inviteCode"
                class="w-full bg-white/5 border border-white/20 rounded-xl px-4 py-3 text-white text-center text-2xl tracking-[0.3em] font-mono uppercase placeholder:text-white/20 placeholder:tracking-normal placeholder:text-base focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50 transition-all"
                placeholder="XXXXXXXX"
                maxlength="8"
                autocomplete="off"
                required
              />
              <p class="mt-2 text-white/40 text-sm text-center">
                Ask your team admin for the 8-character code
              </p>
            </div>

            <!-- Actions -->
            <div class="flex gap-3">
              <button
                type="button"
                (click)="onCancel()"
                class="flex-1 px-4 py-3 border border-white/20 rounded-xl font-semibold text-white/80 hover:bg-white/10 transition-colors"
              >
                Cancel
              </button>
              <button
                type="submit"
                [disabled]="isSubmitting() || inviteCode.length < 8"
                class="flex-1 px-4 py-3 bg-primary hover:bg-primary/90 rounded-xl font-semibold text-white transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                @if (isSubmitting()) {
                  <span class="flex items-center justify-center gap-2">
                    <svg
                      class="animate-spin h-5 w-5"
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                    >
                      <circle
                        class="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        stroke-width="4"
                      ></circle>
                      <path
                        class="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      ></path>
                    </svg>
                    Joining...
                  </span>
                } @else {
                  Join Team
                }
              </button>
            </div>
          </form>
        }
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JoinTeamDialogComponent {
  private readonly teamService = inject(TeamService);

  @Output() cancelled = new EventEmitter<void>();
  @Output() joined = new EventEmitter<TeamDto>();

  inviteCode = '';
  readonly isSubmitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly joinedTeam = signal<TeamDto | null>(null);

  onCancel(): void {
    this.cancelled.emit();
  }

  onSubmit(): void {
    if (this.inviteCode.length < 8) return;

    this.isSubmitting.set(true);
    this.error.set(null);

    this.teamService.joinTeamByCode({ inviteCode: this.inviteCode }).subscribe({
      next: (team) => {
        this.isSubmitting.set(false);
        this.joinedTeam.set(team);
        this.joined.emit(team);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.error.set(err.message || 'Failed to join team');
      },
    });
  }
}
