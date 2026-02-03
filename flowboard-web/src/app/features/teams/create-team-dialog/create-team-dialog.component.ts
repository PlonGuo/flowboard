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
  selector: 'app-create-team-dialog',
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
          <h2 class="text-2xl font-bold text-white">Create a Team</h2>
          <p class="text-white/50 mt-2">
            Create a new team to collaborate with others
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
        @if (createdTeam()) {
          <div class="text-center py-6">
            <div
              class="size-16 rounded-full bg-green-500/20 flex items-center justify-center mx-auto mb-4"
            >
              <span class="material-symbols-outlined text-4xl text-green-400"
                >check_circle</span
              >
            </div>
            <h3 class="text-xl font-bold text-white mb-2">Team Created!</h3>
            <p class="text-white/60 mb-1">
              Your team <span class="text-primary font-semibold">{{ createdTeam()?.name }}</span> is ready
            </p>
            @if (createdTeam()?.inviteCode) {
              <div class="mt-4 p-3 bg-white/5 rounded-lg">
                <p class="text-white/40 text-xs uppercase tracking-wider mb-1">
                  Invite Code
                </p>
                <p class="text-white font-mono text-xl tracking-wider">
                  {{ createdTeam()?.inviteCode }}
                </p>
                <p class="text-white/40 text-xs mt-2">
                  Share this code with your team members
                </p>
              </div>
            }
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
            <div class="space-y-4 mb-6">
              <div>
                <label
                  for="teamName"
                  class="block text-sm font-semibold text-white/80 mb-2"
                  >Team Name *</label
                >
                <input
                  id="teamName"
                  type="text"
                  [(ngModel)]="teamName"
                  name="teamName"
                  class="w-full bg-white/5 border border-white/20 rounded-xl px-4 py-3 text-white placeholder:text-white/30 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50 transition-all"
                  placeholder="e.g., Product Team"
                  maxlength="100"
                  required
                />
              </div>
              <div>
                <label
                  for="teamDescription"
                  class="block text-sm font-semibold text-white/80 mb-2"
                  >Description</label
                >
                <textarea
                  id="teamDescription"
                  [(ngModel)]="teamDescription"
                  name="teamDescription"
                  rows="3"
                  class="w-full bg-white/5 border border-white/20 rounded-xl px-4 py-3 text-white placeholder:text-white/30 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50 transition-all resize-none"
                  placeholder="Brief description of your team..."
                  maxlength="500"
                ></textarea>
              </div>
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
                [disabled]="isSubmitting() || !teamName.trim()"
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
                    Creating...
                  </span>
                } @else {
                  Create Team
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
export class CreateTeamDialogComponent {
  private readonly teamService = inject(TeamService);

  @Output() cancelled = new EventEmitter<void>();
  @Output() created = new EventEmitter<TeamDto>();

  teamName = '';
  teamDescription = '';
  readonly isSubmitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly createdTeam = signal<TeamDto | null>(null);

  onCancel(): void {
    this.cancelled.emit();
  }

  onSubmit(): void {
    if (!this.teamName.trim()) return;

    this.isSubmitting.set(true);
    this.error.set(null);

    this.teamService
      .createTeam({
        name: this.teamName.trim(),
        description: this.teamDescription.trim() || undefined,
      })
      .subscribe({
        next: (team) => {
          this.isSubmitting.set(false);
          this.createdTeam.set(team);
          this.created.emit(team);
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.error.set(err.message || 'Failed to create team');
        },
      });
  }
}
