
import { ChangeDetectionStrategy, Component, output } from '@angular/core';

@Component({
  selector: 'app-ai-chat',
  template: `
<div class="fixed inset-0 bg-black/40 z-50"></div>
<div class="fixed bottom-8 right-8 z-50 pointer-events-none">
  <div class="flex flex-col items-end gap-4 max-w-md w-full pointer-events-auto">
    <div class="glass-panel w-full h-[600px] rounded-xl flex flex-col overflow-hidden animate-slide-in-from-bottom">
      <div class="p-6 border-b border-white/10 flex items-center justify-between bg-white/5">
        <div class="flex items-center gap-3">
          <div class="size-10 rounded-full bg-gradient-to-tr from-primary to-blue-400 flex items-center justify-center relative">
            <span class="material-symbols-outlined text-white">smart_toy</span>
            <div class="absolute bottom-0 right-0 size-3 bg-green-500 rounded-full border-2 border-background-dark"></div>
          </div>
          <div>
            <h4 class="font-bold text-lg leading-none">FlowBoard AI</h4>
            <p class="text-xs text-white/50 mt-1">Your collaborative assistant</p>
          </div>
        </div>
        <div class="flex gap-2">
          <button (click)="onClose()" class="size-8 rounded-full flex items-center justify-center hover:bg-white/10"><span class="material-symbols-outlined text-sm">close</span></button>
        </div>
      </div>

      <div class="flex-1 overflow-y-auto p-6 space-y-6">
        <div class="flex gap-3 max-w-[85%]">
          <div class="glass-ai-bubble p-4 rounded-xl rounded-tl-none"><p class="text-sm leading-relaxed">Hello! I've analyzed your "Project Alpha" board. You have 3 overdue tasks in the Review column. Would you like me to ping the assignees or suggest a reshuffle?</p></div>
        </div>
        <div class="flex gap-3 max-w-[85%] ml-auto justify-end">
          <div class="glass-user-bubble p-4 rounded-xl rounded-tr-none"><p class="text-sm leading-relaxed">Can you summarize the status of the current sprint? We have a stakeholder meeting in 10 minutes.</p></div>
        </div>
      </div>
      
      <div class="p-6 pt-2">
        <div class="glass-input rounded-xl flex items-center p-2 group">
          <input class="bg-transparent border-none focus:ring-0 text-sm flex-1 placeholder:text-white/20" placeholder="Ask FlowBoard anything..." type="text"/>
          <button class="size-10 bg-primary rounded-lg flex items-center justify-center text-white shadow-lg shadow-primary/20"><span class="material-symbols-outlined">send</span></button>
        </div>
      </div>
    </div>
  </div>
</div>
  `,
  styles: [`
.glass-panel {
    background: rgba(25, 26, 46, 0.7);
    backdrop-filter: blur(40px);
    -webkit-backdrop-filter: blur(40px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
}
.glass-ai-bubble {
    background: rgba(100, 103, 242, 0.15);
    backdrop-filter: blur(12px);
    border: 1px solid rgba(100, 103, 242, 0.3);
}
.glass-user-bubble {
    background: rgba(59, 130, 246, 0.1);
    backdrop-filter: blur(12px);
    border: 1px solid rgba(59, 130, 246, 0.2);
}
.glass-input {
    background: rgba(0, 0, 0, 0.2);
    box-shadow: inset 0 2px 4px 0 rgba(0, 0, 0, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.05);
}
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AiChatComponent {
  closeChat = output<void>();

  onClose() {
    this.closeChat.emit();
  }
}
