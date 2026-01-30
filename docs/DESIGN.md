# FlowBoard - Complete Technical Design Document

> Last Updated: January 2026  
> Version: 1.0.0

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [System Architecture](#3-system-architecture)
4. [Core Features](#4-core-features)
5. [Database Design](#5-database-design)
6. [API Design](#6-api-design)
7. [Technical Implementation](#7-technical-implementation)
8. [Technical Challenges](#8-technical-challenges)
9. [Development Roadmap](#9-development-roadmap)
10. [Performance Metrics](#10-performance-metrics)
11. [Cost Estimation](#11-cost-estimation)
12. [Future Expansion](#12-future-expansion)

---

## 1. Project Overview

### 1.1 Project Positioning
**FlowBoard** is a real-time collaboration platform designed for small and medium teams, with kanban management as the core feature, supporting multi-user real-time collaboration, task tracking, and team communication.

### 1.2 Target Users
- Small startup teams (5-20 people)
- Remote collaboration teams
- Agile development teams
- Project managers

### 1.3 Core Value Proposition
- ✅ **Real-time Collaboration** - Multiple users operate simultaneously, changes visible immediately
- ✅ **Simple & Intuitive** - Drag-and-drop interface, zero learning curve
- ✅ **Ready to Use** - No complex configuration needed
- ✅ **Excellent Performance** - Supports large numbers of tasks and concurrent users

---

## 2. Technology Stack

### 2.1 Backend Stack
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Main framework |
| ASP.NET Core Web API | 8.0 | RESTful API |
| SignalR | 8.0 | WebSocket real-time communication |
| Entity Framework Core | 8.0 | ORM data access |
| SQL Server | 2022 | Relational database |
| MediatR | 12.x | CQRS + Event-driven |
| Serilog | 3.x | Structured logging |
| FluentValidation | 11.x | Data validation |
| AutoMapper | 12.x | Object mapping |
| JWT | - | Authentication |
| **Semantic Kernel** | **1.x** | **AI framework** |
| **Azure OpenAI** | **latest** | **AI service** |

**Technology Rationale:**
- **SignalR**: .NET native real-time communication solution, deeply integrated with ASP.NET Core
- **MediatR**: Decouples business logic, implements event-driven architecture
- **Serilog**: Production-grade logging solution supporting structured logging and multiple outputs
- **Semantic Kernel**: Microsoft's official AI framework, seamlessly integrated with .NET

### 2.2 Frontend Stack
| Technology | Version | Purpose |
|------------|---------|---------|
| Angular | 17.x | Frontend framework |
| Angular Material | 17.x | UI component library |
| Angular CDK | 17.x | Drag-and-drop functionality |
| RxJS | 7.x | Reactive programming |
| @microsoft/signalr | 8.x | SignalR client |
| TypeScript | 5.x | Type safety |
| **Excalidraw** | **latest** | **Collaborative whiteboard** |
| **React/ReactDOM** | **18.x** | **Excalidraw dependency** |

**Technology Rationale:**
- **Angular CDK**: Provides powerful drag-and-drop API
- **RxJS**: Perfect for handling real-time data streams and asynchronous operations
- **TypeScript**: Type safety reduces runtime errors
- **Excalidraw**: Open-source collaborative whiteboard with hand-drawn style and built-in real-time collaboration capabilities

### 2.3 Deployment Solution
| Service | Platform | Cost |
|---------|----------|------|
| Web API + SignalR | Azure App Service (B1) | ~$13/month |
| Database | Azure SQL Database (Basic) | ~$5/month |
| Frontend Hosting | Azure App Service (included above) | Included |
| Storage | Azure Blob Storage | ~$2/month |
| **AI Service** | **Azure OpenAI** | **~$5-10/month** |

**Total Cost**: ~$25-30/month (4-5 months with $100 student credit)

---

## 3. System Architecture

### 3.1 Overall Architecture Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                        User Browser                           │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Angular SPA                              │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │   │
│  │  │  Board   │  │   Task   │  │  Notify  │           │   │
│  │  └──────────┘  └──────────┘  └──────────┘           │   │
│  │         │              │              │               │   │
│  │         └──────────────┴──────────────┘               │   │
│  │                     │                                 │   │
│  │            ┌────────┴────────┐                        │   │
│  │            │  SignalR Client │                        │   │
│  │            └────────┬────────┘                        │   │
│  └─────────────────────┼─────────────────────────────────┘   │
└────────────────────────┼──────────────────────────────────────┘
                         │ WebSocket
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                    Azure App Service                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           ASP.NET Core Application                    │   │
│  │  ┌────────────────┐         ┌────────────────┐       │   │
│  │  │  Controllers   │ ←────→  │  SignalR Hubs  │       │   │
│  │  │  (REST API)    │         │  (WebSocket)   │       │   │
│  │  └────────┬───────┘         └────────┬───────┘       │   │
│  │           │                          │               │   │
│  │           └──────────┬───────────────┘               │   │
│  │                      ↓                               │   │
│  │           ┌──────────────────┐                       │   │
│  │           │  MediatR Handler │                       │   │
│  │           │  (Business Logic) │                      │   │
│  │           └──────────┬───────┘                       │   │
│  │                      ↓                               │   │
│  │           ┌──────────────────┐                       │   │
│  │           │  EF Core DbContext│                      │   │
│  │           └──────────┬───────┘                       │   │
│  └──────────────────────┼────────────────────────────────┘   │
└────────────────────────┼──────────────────────────────────────┘
                         │
                         ↓
              ┌─────────────────────┐
              │  Azure SQL Database │
              │  (Relational DB)     │
              └─────────────────────┘
```

### 3.2 Real-time Communication Flow
```
User A drags task
    ↓
Angular sends SignalR message
    ↓
SignalR Hub receives
    ↓
Calls MediatR Handler
    ↓
Updates database (with version number)
    ↓
SignalR broadcasts to other users in group
    ↓
Users B/C/D browsers update UI in real-time
```

### 3.3 Project Structure

#### Backend Project Structure
```
src/
├── FlowBoard.API/                    # Web API + SignalR main project
│   ├── Controllers/                  # REST API controllers
│   │   ├── AuthController.cs
│   │   ├── BoardsController.cs
│   │   ├── TasksController.cs
│   │   └── TeamsController.cs
│   ├── Hubs/                         # SignalR Hubs
│   │   ├── BoardHub.cs
│   │   └── CanvasHub.cs
│   ├── Filters/                      # Global filters
│   ├── Middleware/                   # Custom middleware
│   ├── Extensions/                   # Extension methods
│   └── Program.cs
│
├── FlowBoard.Core/                   # Core domain layer
│   ├── Entities/                     # Entity models
│   │   ├── User.cs
│   │   ├── Team.cs
│   │   ├── Board.cs
│   │   ├── Column.cs
│   │   ├── Task.cs
│   │   ├── Comment.cs
│   │   └── Canvas.cs
│   ├── Enums/                        # Enumerations
│   ├── Interfaces/                   # Interface definitions
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Specifications/               # Specification pattern
│
├── FlowBoard.Application/            # Application layer
│   ├── Commands/                     # CQRS commands
│   │   ├── CreateTaskCommand.cs
│   │   ├── MoveTaskCommand.cs
│   │   └── UpdateTaskCommand.cs
│   ├── Queries/                      # CQRS queries
│   │   ├── GetBoardQuery.cs
│   │   └── GetTasksQuery.cs
│   ├── Handlers/                     # MediatR handlers
│   ├── DTOs/                         # Data transfer objects
│   ├── Validators/                   # FluentValidation
│   ├── Mappings/                     # AutoMapper configuration
│   └── Events/                       # Domain events
│       ├── TaskCreatedEvent.cs
│       └── TaskMovedEvent.cs
│
├── FlowBoard.Infrastructure/         # Infrastructure layer
│   ├── Data/                         # Data access
│   │   ├── FlowBoardDbContext.cs
│   │   ├── Configurations/           # EF Core configuration
│   │   ├── Repositories/             # Repository implementations
│   │   └── Migrations/               # Database migrations
│   ├── Services/                     # Infrastructure services
│   │   ├── CacheService.cs
│   │   ├── NotificationService.cs
│   │   ├── AIAssistantService.cs
│   │   └── SignalRConnectionManager.cs
│   └── Identity/                     # Authentication
│
└── FlowBoard.Tests/                  # Test projects
    ├── UnitTests/
    └── IntegrationTests/
```

#### Frontend Project Structure
```
flowboard-web/
├── src/
│   ├── app/
│   │   ├── core/                     # Core module (singletons)
│   │   │   ├── services/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── signalr.service.ts
│   │   │   │   └── cache.service.ts
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── can-deactivate.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   ├── error.interceptor.ts
│   │   │   │   └── loading.interceptor.ts
│   │   │   └── core.module.ts
│   │   │
│   │   ├── shared/                   # Shared module
│   │   │   ├── components/
│   │   │   │   ├── loading-spinner/
│   │   │   │   ├── confirmation-dialog/
│   │   │   │   └── avatar/
│   │   │   ├── directives/
│   │   │   ├── pipes/
│   │   │   └── shared.module.ts
│   │   │
│   │   ├── features/                 # Feature modules (lazy-loaded)
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   ├── register/
│   │   │   │   └── auth.module.ts
│   │   │   │
│   │   │   ├── boards/
│   │   │   │   ├── board-list/
│   │   │   │   ├── board-detail/
│   │   │   │   ├── board-column/
│   │   │   │   ├── task-card/
│   │   │   │   └── boards.module.ts
│   │   │   │
│   │   │   ├── canvas/
│   │   │   │   ├── canvas-list/
│   │   │   │   ├── canvas-editor/
│   │   │   │   └── canvas.module.ts
│   │   │   │
│   │   │   ├── ai/
│   │   │   │   ├── ai-chat-widget/
│   │   │   │   └── ai.module.ts
│   │   │   │
│   │   │   └── teams/
│   │   │       └── teams.module.ts
│   │   │
│   │   ├── layout/                   # Layout components
│   │   │   ├── header/
│   │   │   ├── sidebar/
│   │   │   └── footer/
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.routes.ts
│   │   └── app.config.ts
│   │
│   ├── assets/
│   ├── environments/
│   └── styles/
│
├── angular.json
├── package.json
└── tsconfig.json
```

---

## 4. Core Features

### 4.1 MVP Features (Phase 1)

#### User System
- [ ] User registration (email + password)
- [ ] User login (JWT Token)
- [ ] Password recovery
- [ ] Profile editing
- [ ] Avatar upload

#### Team Management
- [ ] Create team
- [ ] Invite members (email invitation)
- [ ] View member list
- [ ] Basic roles (Owner / Member)
- [ ] Remove members

#### Kanban System ⭐ Core Feature
- [ ] Create board
- [ ] View board list
- [ ] Delete board
- [ ] Create columns (default: To Do/In Progress/Done)
- [ ] Edit column names
- [ ] Delete columns
- [ ] Column sorting

#### Task Management ⭐ Core Feature
- [ ] Create task cards
- [ ] Edit task details
  - [ ] Title
  - [ ] Rich text description
  - [ ] Assignee
  - [ ] Priority (High/Medium/Low)
  - [ ] Due date
- [ ] Drag task cards (within column + across columns)
- [ ] Delete tasks
- [ ] Task filtering (by assignee/priority)
- [ ] Task search

#### Real-time Collaboration ⭐ Core Feature
- [ ] Multiple users viewing board simultaneously
- [ ] Real-time display of online users
- [ ] Real-time task drag synchronization
- [ ] Real-time task edit synchronization
- [ ] Cursor position display (optional)

#### Comment System
- [ ] Task comments
- [ ] @mention users
- [ ] Real-time comment sync

#### Notification System
- [ ] Task assignment notifications
- [ ] Task status change notifications
- [ ] @mention notifications
- [ ] Browser notification popups
- [ ] Notification center (unread markers)

#### AI Smart Assistant ⭐ MVP New Feature
- [ ] Natural language task creation
- [ ] Smart task queries
- [ ] Intelligent task assignment
- [ ] Work summary generation
- [ ] Chat interface (bottom-right floating window)

#### Collaborative Whiteboard ⭐ MVP New Feature
- [ ] Real-time collaborative drawing
- [ ] Flowchart/architecture diagram creation
- [ ] Arrow connections
- [ ] Shape library (rectangle/circle/diamond)
- [ ] Text annotations
- [ ] Real-time cursor display
- [ ] Online user list
- [ ] Auto-save
- [ ] Export images (PNG/SVG)

#### Technical Features
- [ ] IMemoryCache caching strategy
- [ ] Serilog structured logging
- [ ] Angular lazy-loaded modules
- [ ] CanDeactivate guard (prevent unsaved exit)

### 4.2 Extended Features (Post-MVP)

#### Collaborative Document Editing
- [ ] Rich text collaborative editor
- [ ] Multi-user real-time cursors
- [ ] Version history

#### File Management
- [ ] Attachment upload (Azure Blob)
- [ ] File preview
- [ ] Version management

#### Activity Timeline
- [ ] Board activity log
- [ ] Task change history
- [ ] Activity filtering

#### Sprint Management
- [ ] Sprint creation
- [ ] Sprint planning
- [ ] Burndown chart
- [ ] Gantt chart

#### Data Statistics
- [ ] Task statistics charts
- [ ] Team workload analysis
- [ ] Export reports (PDF/Excel)

#### Advanced Permissions
- [ ] Custom roles
- [ ] Fine-grained permission control
- [ ] Board-level permissions

#### AI Assistant Advanced Features
- [ ] Intelligent recommendations (task priority, assignees)
- [ ] Meeting notes generation
- [ ] Risk warnings
- [ ] Python microservice migration (optional)

#### Whiteboard Advanced Features
- [ ] Whiteboard version history
- [ ] Template library (system architecture/UML/flowcharts)
- [ ] Voice/video call integration
- [ ] Collaborative annotations

---

## 5. Database Design

### 5.1 ER Diagram
```
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│    User     │────────<│  TeamMember │>────────│    Team     │
└─────────────┘         └─────────────┘         └─────────────┘
      │                                                │
      │ 1:N                                          1:N│
      ↓                                                ↓
┌─────────────┐                               ┌─────────────┐
│   Comment   │                               │    Board    │
└─────────────┘                               └─────────────┘
      ↑                                                │
      │ N:1                                          1:N│
      │                                                ↓
┌─────────────┐         ┌─────────────┐         ┌─────────────┐
│    Task     │>────────│   Column    │<────────│BoardColumn  │
└─────────────┘   N:1   └─────────────┘   1:N   └─────────────┘
      │
      │ N:1
      ↓
┌─────────────┐
│    User     │
│ (Assignee)  │
└─────────────┘
```

### 5.2 Detailed Table Structures

#### Users Table
```sql
CREATE TABLE Users (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Email           NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(MAX) NOT NULL,
    FullName        NVARCHAR(100) NOT NULL,
    AvatarUrl       NVARCHAR(500),
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt     DATETIME2,
    IsActive        BIT NOT NULL DEFAULT 1,
    
    INDEX IX_Users_Email (Email)
);
```

#### Teams Table
```sql
CREATE TABLE Teams (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500),
    OwnerId         INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (OwnerId) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_Teams_OwnerId (OwnerId)
);
```

#### TeamMembers Table
```sql
CREATE TABLE TeamMembers (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    TeamId          INT NOT NULL,
    UserId          INT NOT NULL,
    Role            NVARCHAR(20) NOT NULL CHECK (Role IN ('Owner', 'Member')),
    JoinedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    UNIQUE (TeamId, UserId),
    INDEX IX_TeamMembers_TeamId (TeamId),
    INDEX IX_TeamMembers_UserId (UserId)
);
```

#### Boards Table
```sql
CREATE TABLE Boards (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500),
    TeamId          INT NOT NULL,
    CreatedById     INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_Boards_TeamId (TeamId)
);
```

#### Columns Table
```sql
CREATE TABLE Columns (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    BoardId         INT NOT NULL,
    Position        INT NOT NULL,
    WipLimit        INT,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (BoardId) REFERENCES Boards(Id) ON DELETE CASCADE,
    INDEX IX_Columns_BoardId (BoardId),
    INDEX IX_Columns_Position (BoardId, Position)
);
```

#### Tasks Table
```sql
CREATE TABLE Tasks (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Title           NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX),
    ColumnId        INT NOT NULL,
    Position        INT NOT NULL,
    AssigneeId      INT,
    Priority        NVARCHAR(20) NOT NULL CHECK (Priority IN ('Low', 'Medium', 'High')),
    DueDate         DATETIME2,
    CreatedById     INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RowVersion      ROWVERSION,
    
    FOREIGN KEY (ColumnId) REFERENCES Columns(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssigneeId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (CreatedById) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_Tasks_ColumnId (ColumnId),
    INDEX IX_Tasks_AssigneeId (AssigneeId),
    INDEX IX_Tasks_Position (ColumnId, Position)
);
```

#### Comments Table
```sql
CREATE TABLE Comments (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Content         NVARCHAR(2000) NOT NULL,
    TaskId          INT NOT NULL,
    AuthorId        INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsEdited        BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id) ON DELETE CASCADE,
    FOREIGN KEY (AuthorId) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_Comments_TaskId (TaskId),
    INDEX IX_Comments_CreatedAt (CreatedAt DESC)
);
```

#### Notifications Table
```sql
CREATE TABLE Notifications (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    UserId          INT NOT NULL,
    Type            NVARCHAR(50) NOT NULL,
    Title           NVARCHAR(200) NOT NULL,
    Message         NVARCHAR(500),
    RelatedTaskId   INT,
    RelatedBoardId  INT,
    IsRead          BIT NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ReadAt          DATETIME2,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (RelatedTaskId) REFERENCES Tasks(Id) ON DELETE SET NULL,
    FOREIGN KEY (RelatedBoardId) REFERENCES Boards(Id) ON DELETE SET NULL,
    INDEX IX_Notifications_UserId_IsRead (UserId, IsRead),
    INDEX IX_Notifications_CreatedAt (CreatedAt DESC)
);
```

#### ActivityLogs Table
```sql
CREATE TABLE ActivityLogs (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    BoardId         INT NOT NULL,
    UserId          INT NOT NULL,
    Action          NVARCHAR(50) NOT NULL,
    EntityType      NVARCHAR(50) NOT NULL,
    EntityId        INT NOT NULL,
    Details         NVARCHAR(MAX),
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (BoardId) REFERENCES Boards(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_ActivityLogs_BoardId_CreatedAt (BoardId, CreatedAt DESC)
);
```

#### Canvases Table
```sql
CREATE TABLE Canvases (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    Name            NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500),
    BoardId         INT,
    TeamId          INT NOT NULL,
    CreatedById     INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (BoardId) REFERENCES Boards(Id) ON DELETE SET NULL,
    FOREIGN KEY (TeamId) REFERENCES Teams(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_Canvases_TeamId (TeamId),
    INDEX IX_Canvases_BoardId (BoardId)
);
```

#### CanvasData Table
```sql
CREATE TABLE CanvasData (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    CanvasId        INT NOT NULL,
    Elements        NVARCHAR(MAX) NOT NULL,
    AppState        NVARCHAR(MAX),
    Files           NVARCHAR(MAX),
    Version         INT NOT NULL DEFAULT 1,
    UpdatedById     INT NOT NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CanvasId) REFERENCES Canvases(Id) ON DELETE CASCADE,
    FOREIGN KEY (UpdatedById) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_CanvasData_CanvasId (CanvasId)
);
```

#### CanvasOperations Table
```sql
CREATE TABLE CanvasOperations (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    CanvasId        INT NOT NULL,
    UserId          INT NOT NULL,
    OperationType   NVARCHAR(50) NOT NULL,
    ElementId       NVARCHAR(100) NOT NULL,
    OperationData   NVARCHAR(MAX) NOT NULL,
    Timestamp       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (CanvasId) REFERENCES Canvases(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE NO ACTION,
    INDEX IX_CanvasOps_CanvasId_Timestamp (CanvasId, Timestamp)
);
```

#### AIChatHistory Table
```sql
CREATE TABLE AIChatHistory (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    UserId          INT NOT NULL,
    BoardId         INT,
    Message         NVARCHAR(2000) NOT NULL,
    Response        NVARCHAR(MAX) NOT NULL,
    ActionTaken     NVARCHAR(100),
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (BoardId) REFERENCES Boards(Id) ON DELETE SET NULL,
    INDEX IX_AIChatHistory_UserId (UserId),
    INDEX IX_AIChatHistory_CreatedAt (CreatedAt DESC)
);
```

---

## 6. API Design

### 6.1 RESTful API Endpoints

#### Authentication API
```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/refresh-token
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
```

#### Teams API
```
GET    /api/teams
POST   /api/teams
GET    /api/teams/{id}
PUT    /api/teams/{id}
DELETE /api/teams/{id}
GET    /api/teams/{id}/members
POST   /api/teams/{id}/members
DELETE /api/teams/{id}/members/{userId}
```

#### Boards API
```
GET    /api/boards
POST   /api/boards
GET    /api/boards/{id}
PUT    /api/boards/{id}
DELETE /api/boards/{id}
POST   /api/boards/{id}/columns
PUT    /api/boards/{id}/columns/{columnId}
DELETE /api/boards/{id}/columns/{columnId}
```

#### Tasks API
```
POST   /api/tasks
GET    /api/tasks/{id}
PUT    /api/tasks/{id}
DELETE /api/tasks/{id}
PUT    /api/tasks/{id}/move
GET    /api/tasks/{id}/comments
POST   /api/tasks/{id}/comments
```

#### Notifications API
```
GET    /api/notifications
PUT    /api/notifications/{id}/read
PUT    /api/notifications/read-all
DELETE /api/notifications/{id}
```

#### Canvas API
```
GET    /api/canvases
POST   /api/canvases
GET    /api/canvases/{id}
PUT    /api/canvases/{id}
DELETE /api/canvases/{id}
PUT    /api/canvases/{id}/data
GET    /api/canvases/{id}/history
```

#### AI Assistant API
```
POST   /api/ai/chat
GET    /api/ai/history
POST   /api/ai/analyze-board
POST   /api/ai/suggest-assignee
```

### 6.2 SignalR Real-time Events

#### Hub Endpoints
```
ws://api.flowboard.com/hubs/board    # Board real-time Hub
ws://api.flowboard.com/hubs/canvas   # Canvas real-time Hub
```

#### BoardHub - Client → Server
- `JoinBoard(boardId)` - Join board room
- `LeaveBoard(boardId)` - Leave board room
- `UserTyping(taskId, userName)` - User is typing

#### BoardHub - Server → Client
- `TaskMoved` - Task moved
- `TaskCreated` - Task created
- `TaskUpdated` - Task updated
- `TaskDeleted` - Task deleted
- `UserOnline` - User online
- `UserOffline` - User offline
- `UserTyping` - User typing
- `CommentAdded` - Comment added

#### CanvasHub - Client → Server
- `JoinCanvas(canvasId)` - Join canvas room
- `LeaveCanvas(canvasId)` - Leave canvas room
- `BroadcastCanvasChange(elements, appState)` - Broadcast canvas changes
- `BroadcastPointer(x, y)` - Broadcast cursor position

#### CanvasHub - Server → Client
- `CanvasElementsChanged` - Canvas elements changed
- `PointerMoved` - User cursor moved
- `UserJoined` - User joined canvas
- `UserLeft` - User left canvas

---

## 7. Technical Implementation

### 7.1 SignalR Real-time Collaboration

**Key Features:**
- WebSocket long connections
- Group management
- Real-time broadcasting
- Connection state management

### 7.2 MediatR Event-Driven

**Key Features:**
- CQRS pattern
- Command/Query separation
- Domain events
- Handler decoupling

### 7.3 Caching Strategy (IMemoryCache)

**Key Features:**
- In-memory caching
- Expiration policies
- Cache invalidation
- Performance optimization

### 7.4 Serilog Structured Logging

**Key Features:**
- Structured logging
- Multiple sinks (Console, File, Application Insights)
- Log levels
- Context enrichment

### 7.5 Angular Lazy Loading

**Key Features:**
- Module-based lazy loading
- Route-level code splitting
- Performance optimization
- Reduced initial bundle size

### 7.6 CanDeactivate Guard

**Key Features:**
- Prevent unsaved navigation
- Confirmation dialogs
- Data loss prevention
- User experience improvement

### 7.7 AI Smart Assistant (Semantic Kernel)

**Key Features:**
- Natural language understanding
- Task extraction
- Board context awareness
- Azure OpenAI integration

### 7.8 Collaborative Whiteboard (Excalidraw)

**Key Features:**
- Real-time drawing synchronization
- Cursor position sharing
- Shape library
- Export functionality

---

## 8. Technical Challenges

### 8.1 SignalR Large-scale Connection Management ⭐⭐⭐⭐⭐

**Challenge:** 100 users simultaneously editing the same board
- Each drag operation broadcasts to 99 users → message storm
- Connection drops and reconnections → state synchronization issues
- Server restarts → all connections lost

**Optimization Solutions:**

**1. Message Batching and Throttling**
- Batch messages every 100ms
- Reduce broadcast frequency
- Queue management

**2. Group Broadcasting Strategy**
- Broadcast by column instead of entire board
- Only notify users watching specific columns
- Reduce unnecessary message delivery

**3. Connection Recovery and State Sync**
- Track last seen version per user
- Sync missed events on reconnection
- Event store for history

### 8.2 Concurrent Conflict Resolution ⭐⭐⭐⭐⭐

**Challenge:** Alice and Bob simultaneously move Task A to "Done"
- Whose operation takes effect?
- How to notify the failed party?
- How to ensure data consistency?

**Optimization Solutions:**

**1. Optimistic Concurrency Control**
- Use `ROWVERSION` for version tracking
- Compare expected vs. actual version
- Return conflict error with current state

**2. Angular Conflict Handling**
- Optimistic UI updates
- Rollback on 409 Conflict
- Display latest state from server

**3. Operation Queuing (Advanced)**
- Per-task operation queue
- Sequential processing
- Eliminates conflicts

### 8.3 Real-time Performance Optimization ⭐⭐⭐⭐

**Challenge:** Board with 500 task cards
- Slow initial load
- Drag lag
- High memory usage

**Optimization Solutions:**

**1. Virtual Scrolling**
- Render only visible items
- Angular CDK Virtual Scroll
- Reduce DOM nodes

**2. OnPush Change Detection**
- Reduce unnecessary re-renders
- TrackBy functions
- Immutable data patterns

**3. Lazy Load Task Details**
- Load only task summaries initially
- Fetch full details on demand
- Reduce initial payload

**4. Caching Strategy**
- Cache board data (5 minutes)
- Incremental updates
- Smart invalidation

### 8.4 Offline and Network Instability ⭐⭐⭐⭐

**Challenge:** User network disconnects for 30 seconds then recovers
- Operations during disconnect lost?
- How to sync with server?
- How to handle conflicts?

**Optimization Solutions:**

**1. Offline Queue**
- Queue operations when offline
- Persist to IndexedDB
- Sync when online

**2. SignalR Auto-reconnect**
- Automatic reconnection with backoff
- State synchronization on reconnect
- Missed event replay

**3. Optimistic UI Updates**
- Update UI immediately
- Sync in background
- Rollback on failure

### 8.5 Memory Leaks and Resource Management ⭐⭐⭐

**Challenge:** Browser becomes sluggish after long use
- SignalR subscriptions not canceled
- RxJS streams not unsubscribed
- Components update after destroy

**Optimization Solutions:**

**1. Auto-unsubscribe**
- Use `takeUntil(destroy$)` pattern
- Component lifecycle management
- Proper cleanup in ngOnDestroy

**2. SignalR Connection Cleanup**
- Stop connections on destroy
- Remove event listeners
- Clear connection state

**3. Backend Connection Management**
- Track and clean up connections
- Handle disconnections gracefully
- Notify other users of offline status

---

## 9. Development Roadmap

### Phase 1: Basic Architecture (Week 1)
- [ ] Create project structure
- [ ] Configure EF Core + SQL Server
- [ ] Implement JWT authentication
- [ ] Configure Serilog logging
- [ ] Configure IMemoryCache
- [ ] Initialize Angular project
- [ ] Configure lazy-loaded routes
- [ ] Implement CanDeactivate guard

### Phase 2: Core Features (Weeks 2-3)
- [ ] User registration/login
- [ ] Team management
- [ ] Board CRUD
- [ ] Task CRUD
- [ ] Drag-and-drop (Angular CDK)
- [ ] SignalR integration
- [ ] Real-time task synchronization

### Phase 3: Advanced Features (Week 4)
- [ ] Comment system
- [ ] Notification system
- [ ] Concurrent conflict resolution
- [ ] Performance optimization (Virtual Scroll)
- [ ] Offline queue
- [ ] **AI Smart Assistant**
- [ ] **Collaborative Whiteboard**
- [ ] Testing and debugging

### Phase 4: Deployment (End of Week 4)
- [ ] Apply for Azure student benefits
- [ ] Deploy to Azure App Service
- [ ] Configure Azure SQL Database
- [ ] Configure Azure OpenAI
- [ ] Configure Application Insights
- [ ] Production environment testing

---

## 10. Performance Metrics

### MVP Target Metrics
- SignalR message latency < 500ms
- Support 50 concurrent users
- Board load time < 2 seconds
- Smooth drag-and-drop with 500 tasks (60fps)
- AI response time < 3 seconds
- Canvas real-time sync latency < 300ms

### Monitoring Metrics
- Active connections count
- Message throughput
- API response time
- Database query performance
- Memory usage
- AI API call count/cost
- Canvas operation frequency

---

## 11. Cost Estimation

### Azure Student Benefits Usage
- **Development Phase (1-2 months)**: Run locally, cost $0
- **Testing Phase (1 month)**: Deploy for testing, toggle services as needed
- **Running Phase (3-4 months)**: 24/7 running, ~$25-30/month (including AI)
- **Total**: $100 credit lasts 4-5 months

### Cost Breakdown
- App Service (B1): $13/month
- Azure SQL Database (Basic): $5/month
- Azure OpenAI API: $5-10/month
- Blob Storage: $2/month
- **Total**: ~$25-30/month

### Cost-Saving Tips
1. Turn off Azure services during development
2. Use free SignalR tier (20 concurrent)
3. Regularly clean up logs and cache
4. Monitor cost usage
5. Use GPT-3.5 for AI assistant (cheaper)
6. Compress canvas data storage

---

## 12. Future Expansion

### AI Feature Evolution Path
```
Phase 1 (MVP - Now)
├── .NET + Semantic Kernel
├── Azure OpenAI API
└── Integrated in existing API

      ↓ (6 months later, if needed)

Phase 2 (Expansion - Optional)
├── Python Microservice (FastAPI + LangChain)
├── Local Models / Complex RAG
├── Vector Database (Pinecone/Weaviate)
└── API Gateway integration
```

**Migration Triggers (consider if any met):**
1. AI requests > 1000/day
2. Need local models (cost optimization)
3. Need complex RAG (document analysis, knowledge base)
4. Need multi-modal AI (image, voice)

### Whiteboard Feature Expansion
- [ ] Template library (system architecture/UML/flowcharts)
- [ ] Voice/video call integration (Agora/Twilio)
- [ ] Whiteboard version history and playback
- [ ] Collaborative annotations and comments
- [ ] Export to PDF documents
- [ ] Embed in task cards

### Other Expansion Directions
- [ ] Mobile app (Flutter/React Native)
- [ ] Desktop app (Electron)
- [ ] Slack/Teams integration
- [ ] Webhook automation
- [ ] Public API for third-party integration

---

**Project Start Date**: TBD  
**Expected Completion**: 4 weeks (MVP version)  
**Technical Difficulty**: ⭐⭐⭐⭐ (Medium-High)

---

*This design document is a living document and will be updated as the project evolves.*
