
import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { KanbanBoardComponent } from './components/kanban-board/kanban-board.component';
import { WhiteboardComponent } from './components/whiteboard/whiteboard.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent, title: 'FlowBoard - Your Boards' },
  { path: 'board/:id', component: KanbanBoardComponent, title: 'FlowBoard - Kanban Board' },
  { path: 'whiteboard/:id', component: WhiteboardComponent, title: 'FlowBoard - Whiteboard' },
  { path: '**', redirectTo: '', pathMatch: 'full' }
];
