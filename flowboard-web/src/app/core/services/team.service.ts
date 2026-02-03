import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, tap, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TeamDto, CreateTeamRequest, JoinTeamRequest } from '../models/team.model';
import { ApiError } from '../models/board.model';

@Injectable({ providedIn: 'root' })
export class TeamService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/teams`;

  // State signals
  private readonly teamsState = signal<TeamDto[]>([]);
  private readonly loadingState = signal<boolean>(false);
  private readonly errorState = signal<string | null>(null);

  // Public readonly selectors
  readonly teams = this.teamsState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly error = this.errorState.asReadonly();

  /**
   * Load all teams for the current user.
   */
  loadUserTeams(): Observable<TeamDto[]> {
    this.loadingState.set(true);
    this.errorState.set(null);

    return this.http.get<TeamDto[]>(this.apiUrl).pipe(
      tap((teams) => {
        this.teamsState.set(teams);
        this.loadingState.set(false);
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Create a new team.
   */
  createTeam(request: CreateTeamRequest): Observable<TeamDto> {
    return this.http.post<TeamDto>(this.apiUrl, request).pipe(
      tap((team) => {
        this.teamsState.update((teams) => [team, ...teams]);
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Join a team using an invite code.
   */
  joinTeamByCode(request: JoinTeamRequest): Observable<TeamDto> {
    return this.http.post<TeamDto>(`${this.apiUrl}/join`, request).pipe(
      tap((team) => {
        // Add the joined team to the list if not already present
        this.teamsState.update((teams) => {
          const exists = teams.some((t) => t.id === team.id);
          return exists ? teams : [team, ...teams];
        });
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Delete a team (owner only). Deletes all associated boards and tasks.
   */
  deleteTeam(teamId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${teamId}`).pipe(
      tap(() => {
        this.teamsState.update((teams) => teams.filter((t) => t.id !== teamId));
      }),
      catchError((error) => this.handleError(error))
    );
  }

  /**
   * Leave a team (members only).
   */
  leaveTeam(teamId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${teamId}/leave`, {}).pipe(
      tap(() => {
        this.teamsState.update((teams) => teams.filter((t) => t.id !== teamId));
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
    } else if (error.status === 401) {
      errorMessage = 'Please log in to continue';
    } else if (error.status === 0) {
      errorMessage = 'Unable to connect to the server';
    }

    this.errorState.set(errorMessage);
    return throwError(() => ({ message: errorMessage } as ApiError));
  }
}
