# FlowBoard API Specification

> Complete API documentation with request/response examples, error codes, and authentication flows.

---

## Table of Contents

1. [General Guidelines](#1-general-guidelines)
2. [Authentication](#2-authentication)
3. [Error Handling](#3-error-handling)
4. [API Endpoints](#4-api-endpoints)
5. [SignalR Events](#5-signalr-events)
6. [WebSocket Connection](#6-websocket-connection)

---

## 1. General Guidelines

### 1.1 Base URL
```
Development: https://localhost:5001/api
Production: https://api.flowboard.com/api
```

### 1.2 Request Headers
```http
Content-Type: application/json
Authorization: Bearer {jwt_token}
Accept: application/json
```

### 1.3 Response Format

**Success Response:**
```json
{
  "data": { ... },
  "message": "Operation successful",
  "timestamp": "2024-01-29T10:00:00Z"
}
```

**Error Response:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid input",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  },
  "timestamp": "2024-01-29T10:00:00Z"
}
```

### 1.4 Pagination
```
GET /api/tasks?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
```

**Response:**
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalItems": 100,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

---

## 2. Authentication

### 2.1 Register

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe"
}
```

**Response 201:**
```json
{
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "fullName": "John Doe",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600
  }
}
```

**Errors:**
- `400` - Validation error (email format, password strength)
- `409` - Email already exists

---

### 2.2 Login

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response 200:**
```json
{
  "data": {
    "userId": 1,
    "email": "user@example.com",
    "fullName": "John Doe",
    "avatarUrl": "https://...",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600
  }
}
```

**Errors:**
- `401` - Invalid credentials
- `403` - Account inactive

---

### 2.3 Refresh Token

**Endpoint:** `POST /api/auth/refresh-token`

**Request:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response 200:**
```json
{
  "data": {
    "token": "new_jwt_token",
    "refreshToken": "new_refresh_token",
    "expiresIn": 3600
  }
}
```

---

### 2.4 JWT Token Structure

**Payload:**
```json
{
  "sub": "1",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Member",
  "iat": 1640000000,
  "exp": 1640003600
}
```

---

## 3. Error Handling

### 3.1 Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `VALIDATION_ERROR` | 400 | Invalid input data |
| `UNAUTHORIZED` | 401 | Missing or invalid token |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `NOT_FOUND` | 404 | Resource not found |
| `CONFLICT` | 409 | Resource conflict (e.g., email exists, version mismatch) |
| `RATE_LIMIT_EXCEEDED` | 429 | Too many requests |
| `INTERNAL_ERROR` | 500 | Server error |

### 3.2 Validation Error Example

**Request:**
```json
POST /api/tasks
{
  "title": "",
  "columnId": -1
}
```

**Response 400:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "title",
        "message": "Title is required"
      },
      {
        "field": "columnId",
        "message": "ColumnId must be greater than 0"
      }
    ]
  }
}
```

### 3.3 Concurrency Conflict Example

**Request:**
```json
PUT /api/tasks/1/move
{
  "toColumnId": 2,
  "toPosition": 0,
  "rowVersion": "AAAAAAAAB9E="
}
```

**Response 409:**
```json
{
  "error": {
    "code": "CONFLICT",
    "message": "Task has been modified by another user",
    "currentState": {
      "id": 1,
      "columnId": 3,
      "position": 1,
      "rowVersion": "AAAAAAAAB9G=",
      "updatedAt": "2024-01-29T10:05:00Z",
      "updatedBy": "Jane Smith"
    }
  }
}
```

---

## 4. API Endpoints

### 4.1 Teams

#### Create Team
**Endpoint:** `POST /api/teams`

**Request:**
```json
{
  "name": "Development Team",
  "description": "Product development team"
}
```

**Response 201:**
```json
{
  "data": {
    "id": 1,
    "name": "Development Team",
    "description": "Product development team",
    "ownerId": 1,
    "createdAt": "2024-01-29T10:00:00Z"
  }
}
```

#### Get Team
**Endpoint:** `GET /api/teams/{id}`

**Response 200:**
```json
{
  "data": {
    "id": 1,
    "name": "Development Team",
    "description": "Product development team",
    "owner": {
      "id": 1,
      "fullName": "John Doe",
      "email": "john@example.com",
      "avatarUrl": "https://..."
    },
    "members": [
      {
        "id": 2,
        "fullName": "Jane Smith",
        "email": "jane@example.com",
        "role": "Member",
        "joinedAt": "2024-01-29T10:00:00Z"
      }
    ],
    "boardCount": 3,
    "createdAt": "2024-01-29T10:00:00Z"
  }
}
```

#### Invite Member
**Endpoint:** `POST /api/teams/{id}/members`

**Request:**
```json
{
  "email": "newmember@example.com",
  "role": "Member"
}
```

**Response 201:**
```json
{
  "data": {
    "invitationId": "inv_123",
    "email": "newmember@example.com",
    "invitedBy": "John Doe",
    "expiresAt": "2024-02-05T10:00:00Z"
  }
}
```

---

### 4.2 Boards

#### Create Board
**Endpoint:** `POST /api/boards`

**Request:**
```json
{
  "name": "Sprint 1",
  "description": "First sprint of Q1 2024",
  "teamId": 1
}
```

**Response 201:**
```json
{
  "data": {
    "id": 1,
    "name": "Sprint 1",
    "description": "First sprint of Q1 2024",
    "teamId": 1,
    "columns": [
      {
        "id": 1,
        "name": "To Do",
        "position": 0,
        "wipLimit": null
      },
      {
        "id": 2,
        "name": "In Progress",
        "position": 1,
        "wipLimit": 3
      },
      {
        "id": 3,
        "name": "Done",
        "position": 2,
        "wipLimit": null
      }
    ],
    "createdAt": "2024-01-29T10:00:00Z"
  }
}
```

#### Get Board with Tasks
**Endpoint:** `GET /api/boards/{id}`

**Response 200:**
```json
{
  "data": {
    "id": 1,
    "name": "Sprint 1",
    "description": "First sprint of Q1 2024",
    "teamId": 1,
    "columns": [
      {
        "id": 1,
        "name": "To Do",
        "position": 0,
        "wipLimit": null,
        "tasks": [
          {
            "id": 1,
            "title": "Implement login",
            "description": null,
            "priority": "High",
            "position": 0,
            "assignee": {
              "id": 2,
              "fullName": "Jane Smith",
              "avatarUrl": "https://..."
            },
            "dueDate": "2024-02-01T00:00:00Z",
            "commentCount": 3,
            "rowVersion": "AAAAAAAAB9E=",
            "createdAt": "2024-01-29T10:00:00Z"
          }
        ]
      }
    ],
    "onlineUsers": [
      {
        "id": 2,
        "fullName": "Jane Smith",
        "avatarUrl": "https://..."
      }
    ]
  }
}
```

---

### 4.3 Tasks

#### Create Task
**Endpoint:** `POST /api/tasks`

**Request:**
```json
{
  "title": "Fix login bug",
  "description": "Users can't login with special characters in password",
  "columnId": 1,
  "priority": "High",
  "assigneeId": 2,
  "dueDate": "2024-02-01T00:00:00Z"
}
```

**Response 201:**
```json
{
  "data": {
    "id": 1,
    "title": "Fix login bug",
    "description": "Users can't login with special characters in password",
    "columnId": 1,
    "position": 0,
    "priority": "High",
    "assignee": {
      "id": 2,
      "fullName": "Jane Smith",
      "avatarUrl": "https://..."
    },
    "dueDate": "2024-02-01T00:00:00Z",
    "rowVersion": "AAAAAAAAB9E=",
    "createdAt": "2024-01-29T10:00:00Z",
    "createdBy": {
      "id": 1,
      "fullName": "John Doe"
    }
  }
}
```

#### Move Task (Drag and Drop)
**Endpoint:** `PUT /api/tasks/{id}/move`

**Request:**
```json
{
  "toColumnId": 2,
  "toPosition": 0,
  "rowVersion": "AAAAAAAAB9E="
}
```

**Response 200:**
```json
{
  "data": {
    "id": 1,
    "columnId": 2,
    "position": 0,
    "rowVersion": "AAAAAAAAB9F=",
    "updatedAt": "2024-01-29T10:05:00Z"
  }
}
```

**Response 409 (Conflict):**
```json
{
  "error": {
    "code": "CONFLICT",
    "message": "Task has been modified",
    "currentState": {
      "id": 1,
      "columnId": 3,
      "position": 1,
      "rowVersion": "AAAAAAAAB9G="
    }
  }
}
```

#### Update Task
**Endpoint:** `PUT /api/tasks/{id}`

**Request:**
```json
{
  "title": "Fix login bug (updated)",
  "description": "Updated description",
  "priority": "Medium",
  "assigneeId": 3,
  "dueDate": "2024-02-05T00:00:00Z",
  "rowVersion": "AAAAAAAAB9E="
}
```

**Response 200:**
```json
{
  "data": {
    "id": 1,
    "title": "Fix login bug (updated)",
    "description": "Updated description",
    "priority": "Medium",
    "assignee": {
      "id": 3,
      "fullName": "Bob Wilson"
    },
    "dueDate": "2024-02-05T00:00:00Z",
    "rowVersion": "AAAAAAAAB9F=",
    "updatedAt": "2024-01-29T10:10:00Z"
  }
}
```

#### Get Task Details
**Endpoint:** `GET /api/tasks/{id}`

**Response 200:**
```json
{
  "data": {
    "id": 1,
    "title": "Fix login bug",
    "description": "Users can't login with special characters in password",
    "columnId": 1,
    "column": {
      "id": 1,
      "name": "To Do",
      "boardId": 1
    },
    "position": 0,
    "priority": "High",
    "assignee": {
      "id": 2,
      "fullName": "Jane Smith",
      "email": "jane@example.com",
      "avatarUrl": "https://..."
    },
    "dueDate": "2024-02-01T00:00:00Z",
    "rowVersion": "AAAAAAAAB9E=",
    "createdBy": {
      "id": 1,
      "fullName": "John Doe"
    },
    "createdAt": "2024-01-29T10:00:00Z",
    "updatedAt": "2024-01-29T10:00:00Z",
    "comments": [
      {
        "id": 1,
        "content": "I'll work on this",
        "author": {
          "id": 2,
          "fullName": "Jane Smith",
          "avatarUrl": "https://..."
        },
        "createdAt": "2024-01-29T10:05:00Z",
        "isEdited": false
      }
    ]
  }
}
```

---

### 4.4 Comments

#### Add Comment
**Endpoint:** `POST /api/tasks/{taskId}/comments`

**Request:**
```json
{
  "content": "I'll work on this tomorrow @JohnDoe"
}
```

**Response 201:**
```json
{
  "data": {
    "id": 1,
    "content": "I'll work on this tomorrow @JohnDoe",
    "taskId": 1,
    "author": {
      "id": 2,
      "fullName": "Jane Smith",
      "avatarUrl": "https://..."
    },
    "mentions": [
      {
        "id": 1,
        "fullName": "John Doe"
      }
    ],
    "createdAt": "2024-01-29T10:05:00Z",
    "isEdited": false
  }
}
```

---

### 4.5 Canvas (Whiteboard)

#### Create Canvas
**Endpoint:** `POST /api/canvases`

**Request:**
```json
{
  "name": "System Architecture",
  "description": "Q1 2024 system design",
  "teamId": 1,
  "boardId": 1
}
```

**Response 201:**
```json
{
  "data": {
    "id": 1,
    "name": "System Architecture",
    "description": "Q1 2024 system design",
    "teamId": 1,
    "boardId": 1,
    "createdBy": {
      "id": 1,
      "fullName": "John Doe"
    },
    "createdAt": "2024-01-29T10:00:00Z"
  }
}
```

#### Get Canvas Data
**Endpoint:** `GET /api/canvases/{id}`

**Response 200:**
```json
{
  "data": {
    "id": 1,
    "name": "System Architecture",
    "elements": [
      {
        "id": "elem_1",
        "type": "rectangle",
        "x": 100,
        "y": 100,
        "width": 200,
        "height": 100,
        "strokeColor": "#000000",
        "backgroundColor": "#ffffff",
        "text": "API Layer"
      }
    ],
    "appState": {
      "viewBackgroundColor": "#ffffff",
      "zoom": 1
    },
    "files": {},
    "version": 5,
    "updatedAt": "2024-01-29T10:10:00Z"
  }
}
```

#### Save Canvas Data
**Endpoint:** `PUT /api/canvases/{id}/data`

**Request:**
```json
{
  "elements": [...],
  "appState": {...},
  "files": {}
}
```

**Response 200:**
```json
{
  "data": {
    "canvasId": 1,
    "version": 6,
    "updatedAt": "2024-01-29T10:15:00Z"
  }
}
```

---

### 4.6 AI Assistant

#### Chat with AI
**Endpoint:** `POST /api/ai/chat`

**Request:**
```json
{
  "message": "Create a high priority task to fix login bug and assign it to Jane",
  "boardId": 1
}
```

**Response 200:**
```json
{
  "data": {
    "message": "✅ I've created the task 'Fix login bug' in the 'To Do' column with high priority and assigned it to Jane Smith.",
    "action": {
      "type": "TaskCreated",
      "taskId": 1,
      "task": {
        "id": 1,
        "title": "Fix login bug",
        "columnId": 1,
        "priority": "High",
        "assigneeId": 2
      }
    }
  }
}
```

#### Analyze Board
**Endpoint:** `POST /api/ai/analyze-board`

**Request:**
```json
{
  "boardId": 1
}
```

**Response 200:**
```json
{
  "data": {
    "message": "📊 Your board has 15 tasks: 5 high priority, 8 medium, and 2 low. Jane Smith has the most tasks (6). There are 2 overdue tasks that need immediate attention.",
    "statistics": {
      "totalTasks": 15,
      "byPriority": {
        "High": 5,
        "Medium": 8,
        "Low": 2
      },
      "byAssignee": {
        "Jane Smith": 6,
        "Bob Wilson": 4,
        "Unassigned": 5
      },
      "overdueTasks": 2
    }
  }
}
```

---

## 5. SignalR Events

### 5.1 BoardHub Events

#### Server → Client Events

**TaskMoved**
```typescript
{
  taskId: number;
  fromColumnId: number;
  toColumnId: number;
  toPosition: number;
  movedBy: {
    id: number;
    fullName: string;
  };
  rowVersion: string;
  timestamp: string;
}
```

**TaskCreated**
```typescript
{
  task: {
    id: number;
    title: string;
    columnId: number;
    position: number;
    priority: string;
    assignee: {
      id: number;
      fullName: string;
      avatarUrl: string;
    };
    createdBy: {
      id: number;
      fullName: string;
    };
    rowVersion: string;
    createdAt: string;
  }
}
```

**TaskUpdated**
```typescript
{
  taskId: number;
  changes: {
    title?: string;
    description?: string;
    priority?: string;
    assigneeId?: number;
    dueDate?: string;
  };
  updatedBy: {
    id: number;
    fullName: string;
  };
  rowVersion: string;
  timestamp: string;
}
```

**UserOnline**
```typescript
{
  userId: number;
  userName: string;
  avatarUrl: string;
  timestamp: string;
}
```

**UserOffline**
```typescript
{
  userId: number;
  timestamp: string;
}
```

**CommentAdded**
```typescript
{
  comment: {
    id: number;
    content: string;
    taskId: number;
    author: {
      id: number;
      fullName: string;
      avatarUrl: string;
    };
    mentions: Array<{ id: number; fullName: string }>;
    createdAt: string;
  }
}
```

---

### 5.2 CanvasHub Events

#### Server → Client Events

**CanvasElementsChanged**
```typescript
{
  userId: number;
  elements: Array<ExcalidrawElement>;
  appState: ExcalidrawAppState;
  timestamp: string;
}
```

**PointerMoved**
```typescript
{
  userId: number;
  userName: string;
  x: number;
  y: number;
  color: string;
  timestamp: string;
}
```

**UserJoined**
```typescript
{
  userId: number;
  name: string;
  avatarUrl: string;
  color: string;
  timestamp: string;
}
```

**UserLeft**
```typescript
{
  userId: number;
  timestamp: string;
}
```

---

## 6. WebSocket Connection

### 6.1 Connection Setup (TypeScript)

```typescript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
  .withUrl(`${environment.signalrUrl}/board`, {
    accessTokenFactory: () => this.authService.getToken()
  })
  .withAutomaticReconnect([0, 2000, 10000, 30000])
  .configureLogging(LogLevel.Information)
  .build();

// Start connection
await connection.start();

// Join board
await connection.invoke('JoinBoard', boardId);

// Listen to events
connection.on('TaskMoved', (data) => {
  console.log('Task moved:', data);
});

// Leave board
await connection.invoke('LeaveBoard', boardId);

// Stop connection
await connection.stop();
```

### 6.2 Connection Setup (C#)

```csharp
// Hubs/BoardHub.cs
public class BoardHub : Hub
{
    public async Task JoinBoard(int boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"board_{boardId}");
        
        // Notify others
        await Clients.OthersInGroup($"board_{boardId}")
            .SendAsync("UserOnline", new
            {
                UserId = Context.UserIdentifier,
                UserName = Context.User.Identity.Name,
                Timestamp = DateTime.UtcNow
            });
    }
    
    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board_{boardId}");
        
        await Clients.Group($"board_{boardId}")
            .SendAsync("UserOffline", new
            {
                UserId = Context.UserIdentifier,
                Timestamp = DateTime.UtcNow
            });
    }
}
```

---

## 7. Rate Limiting

### 7.1 Rate Limits

| Endpoint | Limit | Window |
|----------|-------|--------|
| POST /api/auth/login | 5 requests | 15 minutes |
| POST /api/auth/register | 3 requests | 1 hour |
| POST /api/ai/chat | 30 requests | 1 minute |
| All other endpoints | 100 requests | 1 minute |

### 7.2 Rate Limit Response

**Response 429:**
```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests",
    "retryAfter": 300
  }
}
```

**Headers:**
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1640003600
Retry-After: 300
```

---

*This API specification is versioned and will be updated as features evolve.*
