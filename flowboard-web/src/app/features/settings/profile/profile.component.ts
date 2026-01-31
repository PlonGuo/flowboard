import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  readonly user = this.authService.user;
  readonly loading = signal(false);
  readonly success = signal(false);
  readonly error = signal<string | null>(null);
  readonly defaultAvatars = signal<string[]>([]);
  readonly selectedAvatar = signal<string | null>(null);

  readonly form: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    customAvatarUrl: [''],
  });

  ngOnInit(): void {
    const currentUser = this.user();
    if (currentUser) {
      this.form.patchValue({
        fullName: currentUser.fullName,
        customAvatarUrl: '',
      });
      this.selectedAvatar.set(currentUser.avatarUrl);
    }

    this.authService.getDefaultAvatars().subscribe({
      next: (avatars) => this.defaultAvatars.set(avatars),
      error: () => {
        // Fallback to local paths if API fails
        this.defaultAvatars.set(
          Array.from({ length: 8 }, (_, i) => `/assets/avatars/avatar-${i + 1}.svg`)
        );
      },
    });
  }

  selectAvatar(url: string): void {
    this.selectedAvatar.set(url);
    this.form.patchValue({ customAvatarUrl: '' });
  }

  onCustomUrlChange(): void {
    const customUrl = this.form.get('customAvatarUrl')?.value;
    if (customUrl) {
      this.selectedAvatar.set(customUrl);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.success.set(false);
    this.error.set(null);

    const avatarUrl =
      this.form.get('customAvatarUrl')?.value || this.selectedAvatar();

    this.authService
      .updateProfile({
        fullName: this.form.get('fullName')?.value,
        avatarUrl: avatarUrl,
      })
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.success.set(true);
          setTimeout(() => this.success.set(false), 3000);
        },
        error: (err) => {
          this.loading.set(false);
          this.error.set(err.error?.message ?? 'Failed to update profile');
        },
      });
  }
}
