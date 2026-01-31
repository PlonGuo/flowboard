export interface Canvas {
  id: number;
  name: string;
  description?: string;
  teamId: number;
  boardId?: number;
  createdById: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CanvasData {
  id: number;
  canvasId: number;
  elements: string; // JSON string of canvas elements
  appState?: string; // JSON string of app state
  files?: string; // JSON string of files
  version: number;
  updatedAt: Date;
}

export interface CanvasElement {
  id: string;
  type: string;
  x: number;
  y: number;
  width?: number;
  height?: number;
  content?: string;
  style?: Record<string, string>;
}

export interface CreateCanvasRequest {
  name: string;
  description?: string;
  teamId: number;
  boardId?: number;
}
