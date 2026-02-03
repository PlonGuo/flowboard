import { UserSummaryDto } from './board.model';

export interface CanvasDto {
  id: number;
  name: string;
  description?: string;
  boardId?: number;
  taskId?: number;
  teamId?: number;
  createdBy: UserSummaryDto;
  createdAt: string;
  updatedAt: string;
}

export interface CanvasDetailDto extends CanvasDto {
  data?: CanvasDataDto;
}

export interface CanvasDataDto {
  id: number;
  canvasId: number;
  elements: string;
  appState?: string;
  files?: string;
  version: number;
  updatedAt: string;
}

export interface SaveCanvasDataRequest {
  elements: string;
  appState?: string;
  files?: string;
}

export interface SaveCanvasDataResponse {
  canvasId: number;
  version: number;
  updatedAt: string;
}

export interface CreateTaskCanvasRequest {
  name: string;
}

export interface CanvasUser {
  userId: string;
  userName: string;
  color: string;
  connectionId: string;
}

export interface SceneUpdateEvent {
  userId: string;
  elements: string;
  appState?: string;
  timestamp: string;
}

export interface PointerUpdateEvent {
  userId: string;
  userName: string;
  color: string;
  pointer: {
    x: number;
    y: number;
    selectedElementId?: string;
  };
  timestamp: string;
}

export interface CanvasUpdatedEvent {
  canvasId: number;
  elements: string;
  appState?: string;
  version: number;
  timestamp: string;
}

export interface CanvasDeletedEvent {
  canvasId: number;
  timestamp: string;
}
