import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/boards/dashboard/dashboard.component').then((m) => m.DashboardComponent),
    title: 'FlowBoard - Your Boards',
  },
  {
    path: 'board/:id',
    loadComponent: () =>
      import('./features/boards/kanban-board/kanban-board.component').then((m) => m.KanbanBoardComponent),
    title: 'FlowBoard - Kanban Board',
  },
  {
    path: 'whiteboard/:id',
    loadComponent: () =>
      import('./features/canvas/whiteboard/whiteboard.component').then((m) => m.WhiteboardComponent),
    title: 'FlowBoard - Whiteboard',
  },
  { path: '**', redirectTo: '', pathMatch: 'full' },
];
