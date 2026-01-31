
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { BoardService } from '../../services/board.service';
import { Board, Column, Task } from '../../models/board.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Component({
  selector: 'app-kanban-board',
  template: `
@if (board(); as currentBoard) {
<div class="relative flex h-screen w-full bg-gradient-to-br from-[#1a1a2e] via-[#101122] to-[#2d1b4d] overflow-hidden">
  <!-- Sidebar Navigation -->
  <aside [routerLink]="['/']" class="flex flex-col w-24 bg-black/20 items-center py-8 z-30 border-r border-white/5 cursor-pointer">
    <div class="mb-10">
      <div class="bg-primary size-12 rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
        <span class="material-symbols-outlined text-white text-3xl">dashboard</span>
      </div>
    </div>
    <nav class="flex flex-col gap-6 flex-1">
      <div class="flex flex-col items-center gap-1 group cursor-pointer">
        <div class="p-3 rounded-full bg-primary/20 text-primary transition-all group-hover:bg-primary group-hover:text-white"><span class="material-symbols-outlined">house</span></div>
      </div>
      <div class="flex flex-col items-center gap-1 group cursor-pointer">
        <div class="p-3 rounded-full bg-white/5 text-white/60 transition-all group-hover:bg-white/10 group-hover:text-white"><span class="material-symbols-outlined">check_circle</span></div>
      </div>
      <div class="flex flex-col items-center gap-1 group cursor-pointer">
        <div class="p-3 rounded-full bg-white/5 text-white/60 transition-all group-hover:bg-white/10 group-hover:text-white"><span class="material-symbols-outlined">trending_up</span></div>
      </div>
      <div class="flex flex-col items-center gap-1 group cursor-pointer">
        <div class="p-3 rounded-full bg-white/5 text-white/60 transition-all group-hover:bg-white/10 group-hover:text-white"><span class="material-symbols-outlined">group</span></div>
      </div>
    </nav>
    <div class="mt-auto">
      <div class="p-3 rounded-full bg-white/5 text-white/60 transition-all hover:bg-white/10 hover:text-white cursor-pointer"><span class="material-symbols-outlined">settings</span></div>
    </div>
  </aside>

  <div class="flex flex-col flex-1 relative">
    <!-- Top Navigation Bar -->
    <header class="bg-black/20 backdrop-blur-md sticky top-0 z-20 flex items-center justify-between px-10 h-20 shrink-0 border-b border-white/5">
      <div class="flex items-center gap-6">
        <h1 class="text-2xl font-bold tracking-tight text-white flex items-center gap-3">
          <span class="material-symbols-outlined text-primary">view_kanban</span>
          {{ currentBoard.title }}
        </h1>
        <div class="h-6 w-px bg-white/10 mx-2"></div>
        <div class="flex -space-x-3 items-center">
            @for(member of currentBoard.members.slice(0,3); track member.avatarUrl) {
                <div class="size-9 rounded-full border-2 border-[#1a1a2e] bg-cover bg-center" [style.background-image]="'url(' + member.avatarUrl + ')'"></div>
            }
            @if(currentBoard.moreMembersCount > 0) {
                <div class="size-9 rounded-full border-2 border-[#1a1a2e] bg-white/10 flex items-center justify-center backdrop-blur-md text-xs font-bold">+{{ currentBoard.moreMembersCount }}</div>
            }
        </div>
      </div>
      <div class="flex items-center gap-4">
        <div class="relative w-64">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-white/40 text-lg">search</span>
          <input class="w-full bg-white/5 border-none rounded-full pl-10 pr-4 py-2 text-sm focus:ring-1 focus:ring-primary text-white placeholder:text-white/30" placeholder="Search tasks..." type="text"/>
        </div>
        <button class="bg-primary hover:bg-primary/90 text-white px-6 py-2 rounded-full text-sm font-semibold transition-all">Share</button>
        <button class="size-10 flex items-center justify-center rounded-full bg-white/5 hover:bg-white/10 text-white transition-all"><span class="material-symbols-outlined">notifications</span></button>
        <div class="size-10 rounded-full bg-cover bg-center border border-white/20" style='background-image: url("https://lh3.googleusercontent.com/aida-public/AB6AXuCvkyX7UUVEtPHZrSQ-JSz_TxpGqIhjKBBm_gH4jUsVKdOZWvJaIPRyocrW0z6t-MCDEkHj0mc0dOHteHon5RqhB-o4d9Lmxv0zq2BI3qTCID4E-gUXHwZK7AlDLt8o_vPnHs4eFUyD0lTEXv5Gggxm_zhZ7e-L7nvajZG28MVLLKpqQnbxeJ1YV6bif68R9ciLZkSdckAszU_yBcuoD_4oLE-wOV4Z8a6_qPF5WAG4iQTUaDXH44z2XmMHPBsEuOGAc63Nu0Q5T2iW");'></div>
      </div>
    </header>

    <!-- Main Kanban Content -->
    <main class="flex-1 overflow-x-auto p-10 flex items-start gap-8">
        @for (column of currentBoard.columns; track column.id) {
            <div class="flex flex-col w-[380px] shrink-0 bg-black/20 rounded-xl p-4 min-h-[500px] border border-white/5">
                <div class="flex items-center justify-between mb-6 px-2">
                    <div class="flex items-center gap-2">
                        <span class="size-2 rounded-full" [class.bg-primary]="column.color === 'primary'" [class.bg-green-400]="column.color === 'green-400'" [class.bg-white/40]="column.color === 'white/40'"></span>
                        <h2 class="text-sm font-bold uppercase tracking-widest text-white/60">{{ column.title }}</h2>
                        <span class="px-2 py-0.5 rounded-full text-[10px]" [class.bg-primary/20]="column.color === 'primary'" [class.text-primary]="column.color === 'primary'" [class.bg-green-400/10]="column.color === 'green-400'" [class.text-green-400]="column.color === 'green-400'" [class.bg-white/10]="column.color === 'white/40'" [class.text-white/70]="column.color === 'white/40'">{{ getTasksForColumn(column).length }}</span>
                    </div>
                    <button class="text-white/40 hover:text-white transition-colors"><span class="material-symbols-outlined">more_horiz</span></button>
                </div>
                <div class="flex flex-col gap-4 overflow-y-auto max-h-[calc(100vh-280px)] pr-1">
                    @for (task of getTasksForColumn(column); track task.id) {
                        <div (click)="openTaskDetails(task)" class="glass-card rounded-xl p-5 cursor-pointer" [class.opacity-60]="task.completed" [class.grayscale-0.3]="task.completed">
                            <div class="flex gap-2 mb-3">
                                @for (tag of task.tags; track tag.name) {
                                    <span class="text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-tighter" [class]="tag.color + ' ' + tag.textColor">{{tag.name}}</span>
                                }
                            </div>
                            <h3 class="text-white font-medium mb-4 leading-relaxed" [class.line-through]="task.completed" [class.text-white/80]="task.completed">{{ task.title }}</h3>
                            @if (task.progress) {
                                <div class="w-full bg-white/5 h-1 rounded-full mb-4 overflow-hidden">
                                    <div class="bg-primary h-full" [style.width.%]="task.progress"></div>
                                </div>
                            }
                            <div class="flex items-center justify-between mt-auto">
                                <div class="flex items-center gap-3 text-white/40">
                                    @if(task.completed) {
                                        <span class="material-symbols-outlined text-green-400">check_circle</span>
                                    }
                                    @if (task.commentsCount > 0) {
                                        <div class="flex items-center gap-1"><span class="material-symbols-outlined text-sm">chat_bubble</span><span class="text-xs">{{task.commentsCount}}</span></div>
                                    }
                                    @if (task.attachmentsCount > 0) {
                                        <div class="flex items-center gap-1"><span class="material-symbols-outlined text-sm">attachment</span><span class="text-xs">{{task.attachmentsCount}}</span></div>
                                    }
                                    @if (task.dueDate) {
                                        <div class="flex items-center gap-1 text-primary"><span class="material-symbols-outlined text-sm">schedule</span><span class="text-xs">{{task.dueDate}}</span></div>
                                    }
                                </div>
                                <div class="flex -space-x-2">
                                    @for(assignee of task.assignees; track assignee.avatarUrl) {
                                        <div class="size-6 rounded-full border border-[#1a1a2e] bg-cover" [style.background-image]="'url(' + assignee.avatarUrl + ')'"></div>
                                    }
                                </div>
                            </div>
                        </div>
                    }
                    <button class="flex items-center justify-center gap-2 p-3 rounded-xl border-2 border-dashed border-white/5 hover:border-white/10 hover:bg-white/5 transition-all text-white/40 hover:text-white/60">
                        <span class="material-symbols-outlined">add</span><span class="text-sm font-medium">Add task</span>
                    </button>
                </div>
            </div>
        }
    </main>
  </div>
</div>
}

<!-- Task Details Modal -->
@if (selectedTask(); as task) {
  <div class="fixed inset-0 bg-black/60 z-40" (click)="closeTaskDetails()"></div>
  <div class="fixed top-0 right-0 h-screen w-full md:w-[600px] lg:w-[700px] z-50 bg-[#191A2E]/80 backdrop-blur-2xl border-l border-white/10 shadow-2xl flex flex-col animate-slide-in-from-right">
    <!-- Header -->
    <header class="flex items-center justify-between px-8 py-6 border-b border-white/10">
      <div class="flex flex-col gap-1">
        <div class="flex items-center gap-3">
          <span class="px-3 py-1 bg-primary/20 text-primary text-xs font-bold rounded-full uppercase tracking-wider border border-primary/30">In Progress</span>
          <span class="text-white/40 text-sm">DEV-402</span>
        </div>
        <h1 class="text-2xl font-bold text-white mt-1">{{ task.title }}</h1>
      </div>
      <button (click)="closeTaskDetails()" class="size-12 rounded-full bg-white/5 border border-white/10 flex items-center justify-center hover:bg-white/10 transition-colors">
        <span class="material-symbols-outlined text-white">close</span>
      </button>
    </header>
    <!-- Content -->
    <div class="flex-1 overflow-y-auto px-8 py-6 space-y-8">
      <section>
        <h2 class="text-lg font-bold flex items-center gap-2"><span class="material-symbols-outlined text-white/60">description</span>Description</h2>
        <div class="bg-white/5 border border-white/10 rounded-2xl p-6 text-white/80 leading-relaxed mt-4">
          {{ task.description }}
        </div>
      </section>
      <section>
        <h2 class="text-lg font-bold flex items-center gap-2"><span class="material-symbols-outlined text-white/60">checklist</span>Checklist</h2>
        <div class="bg-white/5 border border-white/10 rounded-2xl p-6 space-y-4 mt-4">
          <div class="flex flex-col gap-2">
            <div class="flex justify-between text-sm mb-1">
              <span class="text-white/60">Completion</span>
              <span class="text-white font-bold">{{ getChecklistProgress(task) }}%</span>
            </div>
            <div class="h-2 w-full bg-white/5 rounded-full overflow-hidden">
              <div class="h-full bg-primary rounded-full" [style.width.%]="getChecklistProgress(task)"></div>
            </div>
          </div>
          <div class="space-y-3 pt-2">
            @for (item of task.checklist; track item.item) {
              <div class="flex items-center gap-3 group">
                <button class="size-6 rounded-full border-2 flex items-center justify-center" [class.border-primary]="item.completed" [class.bg-primary]="item.completed" [class.border-white/20]="!item.completed">
                  @if (item.completed) { <span class="material-symbols-outlined text-white text-xs">check</span> }
                </button>
                <span class="text-sm" [class.text-white/50]="item.completed" [class.line-through]="item.completed">{{ item.item }}</span>
              </div>
            }
          </div>
        </div>
      </section>
    </div>
    <footer class="p-6 border-t border-white/10 bg-black/20">
      <div class="flex items-center gap-3 bg-black/30 border border-white/10 rounded-full p-1.5 pl-5">
        <input class="bg-transparent border-none focus:ring-0 text-sm flex-1 placeholder:text-white/30 text-white" placeholder="Write a comment or ask AI..." type="text"/>
        <button class="size-10 rounded-full bg-primary flex items-center justify-center hover:scale-105 transition-transform"><span class="material-symbols-outlined text-white">send</span></button>
      </div>
    </footer>
  </div>
}
  `,
  styles: [`
.glass-panel {
    background: rgba(255, 255, 255, 0.05);
    backdrop-filter: blur(16px);
    -webkit-backdrop-filter: blur(16px);
    border: 1px solid rgba(255, 255, 255, 0.1);
}
.glass-card {
    background: rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(8px);
    -webkit-backdrop-filter: blur(8px);
    border: 1px solid rgba(255, 255, 255, 0.15);
    transition: all 0.3s ease;
}
.glass-card:hover {
    background: rgba(255, 255, 255, 0.15);
    border: 1px solid rgba(255, 255, 255, 0.25);
    transform: translateY(-2px);
}
.bg-gradient-main {
    background: linear-gradient(135deg, #1a1a2e 0%, #2d1b4d 50%, #101122 100%);
}
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterModule]
})
export class KanbanBoardComponent {
  private route = inject(ActivatedRoute);
  private boardService = inject(BoardService);

  private boardId = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('id')))
  );

  board = computed(() => {
    const id = this.boardId();
    if (id) {
      return this.boardService.getBoard(id);
    }
    return undefined;
  });

  selectedTask = signal<Task | undefined>(undefined);

  getTasksForColumn(column: Column): Task[] {
    const currentBoard = this.board();
    if (!currentBoard) return [];
    return column.tasks.map(taskId => this.boardService.getTask(currentBoard, taskId)).filter(t => t) as Task[];
  }
  
  openTaskDetails(task: Task) {
    this.selectedTask.set(task);
  }

  closeTaskDetails() {
    this.selectedTask.set(undefined);
  }
  
  getChecklistProgress(task: Task) {
    if (!task.checklist || task.checklist.length === 0) return 0;
    const completedItems = task.checklist.filter(item => item.completed).length;
    return Math.round((completedItems / task.checklist.length) * 100);
  }
}
