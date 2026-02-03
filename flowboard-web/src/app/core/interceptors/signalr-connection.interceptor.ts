import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SignalRService } from '../services/signalr.service';

/**
 * HTTP interceptor that adds the SignalR connection ID header to task and comment API requests.
 * This allows the backend to exclude the sender from receiving their own real-time events.
 */
export const signalrConnectionInterceptor: HttpInterceptorFn = (req, next) => {
  const signalrService = inject(SignalRService);
  const connectionId = signalrService.connectionId();

  // Only add header for task and comment-related API endpoints
  const shouldAddHeader =
    connectionId &&
    (req.url.includes('/api/tasks') || req.url.includes('/api/comments'));

  if (shouldAddHeader) {
    req = req.clone({
      setHeaders: {
        'X-SignalR-Connection-Id': connectionId,
      },
    });
  }

  return next(req);
};
