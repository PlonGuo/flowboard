import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, tap, catchError, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CanvasDetailDto,
  CanvasDto,
  CreateTaskCanvasRequest,
  SaveCanvasDataRequest,
  SaveCanvasDataResponse,
} from '../models/canvas.model';

@Injectable({ providedIn: 'root' })
export class CanvasService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/canvases`;

  // State signals
  private readonly loadingState = signal<boolean>(false);
  private readonly errorState = signal<string | null>(null);

  // Public readonly selectors
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  /**
   * Get canvas for a specific task.
   * Returns null if task has no canvas.
   */
  getTaskCanvas(taskId: number): Observable<CanvasDetailDto | null> {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http.get<CanvasDetailDto>(`${this.apiUrl}/task/${taskId}`, {
      observe: 'response'
    }).pipe(
      map(response => {
        this.loadingState.set(false);
        // 204 No Content means no canvas exists
        if (response.status === 204) {
          return null;
        }
        return response.body;
      }),
      catchError((error: HttpErrorResponse) => {
        this.loadingState.set(false);
        // 204 is success, not error
        if (error.status === 204) {
          return [null];
        }
        return this.handleError(error);
      })
    );
  }

  /**
   * Create a canvas for a task.
   */
  createTaskCanvas(taskId: number, name: string): Observable<CanvasDetailDto> {
    this.loadingState.set(true);
    this.errorState.set(null);

    const request: CreateTaskCanvasRequest = { name };
    return this.http.post<CanvasDetailDto>(`${this.apiUrl}/task/${taskId}`, request).pipe(
      tap(() => this.loadingState.set(false)),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Get canvas by ID.
   */
  getCanvas(canvasId: number): Observable<CanvasDetailDto> {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http.get<CanvasDetailDto>(`${this.apiUrl}/${canvasId}`).pipe(
      tap(() => this.loadingState.set(false)),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Save canvas data.
   */
  saveCanvasData(
    canvasId: number,
    data: SaveCanvasDataRequest
  ): Observable<SaveCanvasDataResponse> {
    return this.http
      .put<SaveCanvasDataResponse>(`${this.apiUrl}/${canvasId}/data`, data)
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Delete a canvas.
   */
  deleteCanvas(canvasId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${canvasId}`).pipe(
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Clear error state.
   */
  clearError(): void {
    this.errorState.set(null);
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    this.loadingState.set(false);

    let errorMessage = 'An unexpected error occurred';
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action';
    } else if (error.status === 404) {
      errorMessage = 'Canvas not found';
    } else if (error.status === 401) {
      errorMessage = 'Please log in to continue';
    } else if (error.status === 0) {
      errorMessage = 'Unable to connect to the server';
    }

    this.errorState.set(errorMessage);
    return throwError(() => ({ message: errorMessage }));
  }
}
