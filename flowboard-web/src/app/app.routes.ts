import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
    canActivate: [guestGuard],
    title: 'FlowBoard - Login',
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
    canActivate: [guestGuard],
    title: 'FlowBoard - Register',
  },
  {
    path: '',
    loadComponent: () =>
      import('./features/boards/dashboard/dashboard.component').then((m) => m.DashboardComponent),
    canActivate: [authGuard],
    title: 'FlowBoard - Your Boards',
  },
  {
    path: 'board/:id',
    loadComponent: () =>
      import('./features/boards/kanban-board/kanban-board.component').then((m) => m.KanbanBoardComponent),
    canActivate: [authGuard],
    title: 'FlowBoard - Kanban Board',
  },
  {
    path: 'board/:boardId/task/:taskId/canvas',
    loadComponent: () =>
      import('./features/canvas/task-canvas/task-canvas.component').then((m) => m.TaskCanvasComponent),
    canActivate: [authGuard],
    title: 'FlowBoard - Task Canvas',
  },
  {
    path: 'whiteboard/:id',
    loadComponent: () =>
      import('./features/canvas/whiteboard/whiteboard.component').then((m) => m.WhiteboardComponent),
    canActivate: [authGuard],
    title: 'FlowBoard - Whiteboard',
  },
  {
    path: 'settings/profile',
    loadComponent: () =>
      import('./features/settings/profile/profile.component').then((m) => m.ProfileComponent),
    canActivate: [authGuard],
    title: 'FlowBoard - Profile Settings',
  },
  { path: '**', redirectTo: '', pathMatch: 'full' },
];
