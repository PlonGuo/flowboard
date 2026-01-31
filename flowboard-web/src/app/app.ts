import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AiChatComponent } from './features/ai/ai-chat/ai-chat.component';
import { UiService } from './core/services/ui.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, AiChatComponent],
  template: `
    <div class="relative flex min-h-screen w-full flex-col overflow-x-hidden">
      <router-outlet></router-outlet>

      @if (uiService.isAiChatVisible()) {
        <app-ai-chat (closeChat)="uiService.toggleAiChat()" />
      }

      <div class="fixed bottom-8 right-8 z-[100]">
        <button
          (click)="uiService.toggleAiChat()"
          class="size-14 rounded-full bg-primary flex items-center justify-center text-white shadow-2xl shadow-primary/40 hover:scale-110 active:scale-95 transition-all"
        >
          <span class="material-symbols-outlined text-3xl">chat_bubble</span>
        </button>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  readonly uiService = inject(UiService);
}
