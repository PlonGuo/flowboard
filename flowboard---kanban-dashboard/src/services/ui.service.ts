
import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class UiService {
  readonly isAiChatVisible = signal(false);

  toggleAiChat() {
    this.isAiChatVisible.update(visible => !visible);
  }
}
