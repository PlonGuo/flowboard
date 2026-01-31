import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, tap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BoardSummaryDto,
  BoardDetailDto,
  CreateBoardRequest,
  UpdateBoardRequest,
  ApiError,
  BoardDto,
} from '../models/board.model';

@Injectable({ providedIn: 'root' })
export class BoardService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/boards`;

  // State signals
  private readonly boardsState = signal<BoardSummaryDto[]>([]);
  private readonly loadingState = signal<boolean>(false);
  private readonly errorState = signal<string | null>(null);

  // Public readonly selectors
  readonly boards = this.boardsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  /**
   * Load all boards for the current user.
   */
  loadUserBoards(): Observable<BoardSummaryDto[]> {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http.get<BoardSummaryDto[]>(this.apiUrl).pipe(
      tap((boards) => {
        this.boardsState.set(boards);
        this.loadingState.set(false);
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Get a specific board by ID with full details.
   */
  getBoard(id: number, includeTasks = true): Observable<BoardDetailDto> {
    return this.http
      .get<BoardDetailDto>(`${this.apiUrl}/${id}`, {
        params: { includeTasks: includeTasks.toString() },
      })
      .pipe(catchError((error) => this.handleError(error)));
  }

  /**
   * Create a new board.
   */
  createBoard(request: CreateBoardRequest): Observable<BoardDetailDto> {
    return this.http.post<BoardDetailDto>(this.apiUrl, request).pipe(
      tap((board) => {
        // Add to local state as summary
        const summary: BoardSummaryDto = {
          id: board.id,
          name: board.name,
          description: board.description,
          taskCount: 0,
          columnCount: board.columns.length,
          updatedAt: board.updatedAt,
        };
        this.boardsState.update((boards) => [summary, ...boards]);
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Update an existing board.
   */
  updateBoard(id: number, request: UpdateBoardRequest): Observable<BoardDto> {
    return this.http.put<BoardDto>(`${this.apiUrl}/${id}`, request).pipe(
      tap((board) => {
        // Update local state
        this.boardsState.update((boards) =>
          boards.map((b) =>
            b.id === id
              ? { ...b, name: board.name, description: board.description, updatedAt: board.updatedAt }
              : b
          )
        );
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Delete a board.
   */
  deleteBoard(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        // Remove from local state
        this.boardsState.update((boards) => boards.filter((b) => b.id !== id));
      }),
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
      errorMessage = 'Board not found';
    } else if (error.status === 401) {
      errorMessage = 'Please log in to continue';
    } else if (error.status === 0) {
      errorMessage = 'Unable to connect to the server';
    }

    this.errorState.set(errorMessage);
    return throwError(() => ({ message: errorMessage } as ApiError));
  }
}
