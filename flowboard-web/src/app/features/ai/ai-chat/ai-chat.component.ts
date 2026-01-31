import { ChangeDetectionStrategy, Component, output } from '@angular/core';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChatComponent {
  closeChat = output<void>();

  onClose(): void {
    this.closeChat.emit();
  }
}
