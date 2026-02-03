import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  TaskItemDto,
  TaskItemDetailDto,
  CreateTaskRequest,
  UpdateTaskRequest,
  MoveTaskRequest,
} from '../models/task.model';

export interface ApiError {
  message: string;
}

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tasks`;

  /**
   * Get a specific task by ID.
   */
  getTask(id: number, includeComments = true): Observable<TaskItemDetailDto> {
    return this.http
      .get<TaskItemDetailDto>(`${this.apiUrl}/${id}`, {
        params: { includeComments: includeComments.toString() },
      })
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Create a new task.
   */
  createTask(request: CreateTaskRequest): Observable<TaskItemDto> {
    return this.http
      .post<TaskItemDto>(this.apiUrl, request)
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Update an existing task.
   */
  updateTask(id: number, request: UpdateTaskRequest): Observable<TaskItemDto> {
    return this.http
      .put<TaskItemDto>(`${this.apiUrl}/${id}`, request)
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Delete a task.
   */
  deleteTask(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`)
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Move a task to a different column and/or position.
   */
  moveTask(id: number, request: MoveTaskRequest): Observable<TaskItemDto> {
    return this.http
      .put<TaskItemDto>(`${this.apiUrl}/${id}/move`, request)
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unexpected error occurred';
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action';
    } else if (error.status === 404) {
      errorMessage = 'Task not found';
    } else if (error.status === 401) {
      errorMessage = 'Please log in to continue';
    } else if (error.status === 409) {
      errorMessage =
        'The task has been modified by another user. Please refresh and try again.';
    } else if (error.status === 0) {
      errorMessage = 'Unable to connect to the server';
    }

    return throwError(() => ({ message: errorMessage } as ApiError));
  }
}
