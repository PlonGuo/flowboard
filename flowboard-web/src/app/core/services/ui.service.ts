import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class UiService {
  readonly isAiChatVisible = signal(false);
  readonly isSidebarCollapsed = signal(false);
  readonly isLoading = signal(false);

  toggleAiChat(): void {
    this.isAiChatVisible.update((visible) => !visible);
  }

  openAiChat(): void {
    this.isAiChatVisible.set(true);
  }

  closeAiChat(): void {
    this.isAiChatVisible.set(false);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.update((collapsed) => !collapsed);
  }

  setLoading(loading: boolean): void {
    this.isLoading.set(loading);
  }
}
