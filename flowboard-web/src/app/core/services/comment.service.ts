import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { CommentDto } from '../models/task.model';

export interface ApiError {
  message: string;
}

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  /**
   * Create a new comment on a task.
   */
  createComment(taskId: number, content: string): Observable<CommentDto> {
    return this.http
      .post<CommentDto>(`${this.apiUrl}/tasks/${taskId}/comments`, { content })
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Update an existing comment.
   */
  updateComment(commentId: number, content: string): Observable<CommentDto> {
    return this.http
      .put<CommentDto>(`${this.apiUrl}/comments/${commentId}`, { content })
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Delete a comment.
   */
  deleteComment(commentId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/comments/${commentId}`)
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unexpected error occurred';
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.status === 403) {
      errorMessage = 'You do not have permission to perform this action';
    } else if (error.status === 404) {
      errorMessage = 'Comment not found';
    } else if (error.status === 401) {
      errorMessage = 'Please log in to continue';
    } else if (error.status === 0) {
      errorMessage = 'Unable to connect to the server';
    }

    return throwError(() => ({ message: errorMessage } as ApiError));
  }
}
