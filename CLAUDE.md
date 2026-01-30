# Claude Code Working Guide for FlowBoard

> Essential instructions for Claude Code when working on the FlowBoard project.

---

## 🎯 Project Overview

FlowBoard is a real-time collaborative kanban board with AI assistant and interactive whiteboard, built with **.NET 8** backend and **Angular 17** frontend.

**Key Technologies:**
- Backend: ASP.NET Core, SignalR, EF Core, SQL Server
- Frontend: Angular 17, TypeScript, RxJS, Angular Material
- Testing: xUnit (backend), Jest/Jasmine (frontend), Cypress (E2E)

---

## 📚 Core Documentation

**ALWAYS read these files before making changes:**
1. `docs/DESIGN.md` - Complete technical design and architecture
2. `docs/API_SPEC.md` - API endpoints, request/response formats, error codes
3. `docs/TESTING.md` - Testing standards and examples
4. `README.md` - Project setup and quick start

---

## 🚨 CRITICAL RULES

### 1. TypeScript Type Safety ⚠️ MANDATORY

**❌ NEVER use `any` type:**
```typescript
// DON'T DO THIS
const data: any = response.data;
function handleEvent(event: any) { }
```

**✅ ALWAYS use proper types:**
```typescript
// DO THIS
interface ApiResponse<T> {
  data: T;
  message?: string;
}

const data: ApiResponse<Task> = response.data;

function handleEvent(event: TaskMovedEvent): void {
  // ...
}
```

**Type Definition Locations:**
- `flowboard-web/src/app/core/models/` - Core domain models
- `flowboard-web/src/app/core/interfaces/` - Service interfaces
- Each feature module has its own types

**Always:**
- Define interfaces for all data structures
- Use strict typing for function parameters and return types
- Enable `strict: true` in `tsconfig.json`
- No `@ts-ignore` or `// @ts-expect-error` without strong justification

---

### 2. Testing Requirements ⚠️ MANDATORY

**Every new feature MUST include tests:**

**Backend (C#):**
- Unit tests for all handlers, validators, and domain logic
- Integration tests for API endpoints
- Minimum 80% code coverage

**Frontend (TypeScript):**
- Unit tests for all components, services, and pipes
- Test both logic and rendering
- Minimum 80% code coverage

**Test file naming:**
- Backend: `{ClassName}Tests.cs` (e.g., `MoveTaskHandlerTests.cs`)
- Frontend: `{component-name}.component.spec.ts`

**Run tests before committing:**
```bash
pnpm test          # Run all tests
pnpm test:back     # Backend only
pnpm test:front    # Frontend only
```

---

### 3. Code Organization

**Backend Structure:**
```
src/
├── FlowBoard.API/           # Controllers, Hubs, Middleware
├── FlowBoard.Core/          # Entities, Interfaces, Enums
├── FlowBoard.Application/   # Commands, Queries, Handlers, DTOs
└── FlowBoard.Infrastructure/ # Data access, External services
```

**Frontend Structure:**
```
flowboard-web/src/app/
├── core/        # Singletons (services, guards, interceptors)
├── shared/      # Reusable components, pipes, directives
├── features/    # Feature modules (lazy-loaded)
└── layout/      # App-wide layout
```

---

## 💻 Development Guidelines

### Backend (C#/.NET)

#### 1. Follow Clean Architecture Principles

**Entities (Core):**
```csharp
// FlowBoard.Core/Entities/Task.cs
public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ColumnId { get; set; }
    public int Position { get; set; }
    public TaskPriority Priority { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Column Column { get; set; } = null!;
    public User? Assignee { get; set; }
    public User CreatedBy { get; set; } = null!;
}
```

#### 2. Use CQRS with MediatR

**Commands:**
```csharp
// FlowBoard.Application/Commands/MoveTaskCommand.cs
public record MoveTaskCommand(
    int TaskId,
    int ToColumnId,
    int ToPosition,
    byte[] RowVersion
) : IRequest<Result<TaskDto>>;

// Handler
public class MoveTaskHandler : IRequestHandler<MoveTaskCommand, Result<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    
    public async Task<Result<TaskDto>> Handle(
        MoveTaskCommand request,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

#### 3. Use FluentValidation

```csharp
// FlowBoard.Application/Validators/CreateTaskCommandValidator.cs
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
            
        RuleFor(x => x.ColumnId)
            .GreaterThan(0).WithMessage("ColumnId must be greater than 0");
    }
}
```

#### 4. API Controllers

**Always:**
- Use `[ApiController]` attribute
- Return `IActionResult` or `ActionResult<T>`
- Include proper HTTP status codes
- Use DTOs, never expose entities directly

```csharp
// FlowBoard.API/Controllers/TasksController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateTask(
        [FromBody] CreateTaskCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new ErrorResponse { Error = result.Error });
            
        return CreatedAtAction(
            nameof(GetTask),
            new { id = result.Value.Id },
            result.Value
        );
    }
}
```

#### 5. Error Handling

**Use Result pattern:**
```csharp
// FlowBoard.Shared/Results/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

---

### Frontend (Angular/TypeScript)

#### 1. Component Structure

**Use standalone components (Angular 17+):**
```typescript
// task-card.component.ts
import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { Task, TaskPriority } from '@app/core/models';

@Component({
  selector: 'app-task-card',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  templateUrl: './task-card.component.html',
  styleUrls: ['./task-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskCardComponent {
  @Input({ required: true }) task!: Task;
  @Output() taskClicked = new EventEmitter<number>();
  @Output() taskMoved = new EventEmitter<{ taskId: number; toColumnId: number }>();
  
  readonly TaskPriority = TaskPriority; // For template
  
  onCardClick(): void {
    this.taskClicked.emit(this.task.id);
  }
  
  onDrop(event: { toColumnId: number; toPosition: number }): void {
    this.taskMoved.emit({
      taskId: this.task.id,
      toColumnId: event.toColumnId
    });
  }
}
```

#### 2. Service Design

**Always:**
- Use proper typing
- Handle errors
- Return Observables from HttpClient
- Implement proper cleanup (unsubscribe)

```typescript
// core/services/board.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '@env/environment';
import { Board, ApiResponse, ErrorResponse } from '@app/core/models';

@Injectable({ providedIn: 'root' })
export class BoardService {
  private readonly apiUrl = `${environment.apiUrl}/boards`;
  
  constructor(private http: HttpClient) {}
  
  getBoard(id: number): Observable<Board> {
    return this.http.get<ApiResponse<Board>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }
  
  createBoard(request: CreateBoardRequest): Observable<Board> {
    return this.http.post<ApiResponse<Board>>(this.apiUrl, request).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }
  
  private handleError(error: HttpErrorResponse): Observable<never> {
    const errorResponse: ErrorResponse = error.error;
    console.error('API Error:', errorResponse);
    return throwError(() => errorResponse);
  }
}
```

#### 3. State Management with RxJS

**Use BehaviorSubject for component state:**
```typescript
// board-detail.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export class BoardDetailComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly boardState$ = new BehaviorSubject<Board | null>(null);
  
  // Public observable for template
  readonly board$ = this.boardState$.asObservable();
  
  ngOnInit(): void {
    this.loadBoard();
    this.setupRealtimeSync();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  private setupRealtimeSync(): void {
    this.signalrService.onTaskMoved$
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => this.handleTaskMoved(event));
  }
}
```

#### 4. Reactive Forms

**Always use typed forms:**
```typescript
// task-form.component.ts
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TaskPriority } from '@app/core/models';

interface TaskFormValue {
  title: string;
  description: string;
  priority: TaskPriority;
  assigneeId: number | null;
  dueDate: Date | null;
}

export class TaskFormComponent {
  readonly form: FormGroup<{
    title: FormControl<string>;
    description: FormControl<string>;
    priority: FormControl<TaskPriority>;
    assigneeId: FormControl<number | null>;
    dueDate: FormControl<Date | null>;
  }>;
  
  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      priority: [TaskPriority.Medium],
      assigneeId: [null],
      dueDate: [null]
    });
  }
  
  onSubmit(): void {
    if (this.form.invalid) return;
    
    const value: TaskFormValue = this.form.value as TaskFormValue;
    // Submit value
  }
}
```

---

## 🔍 Common Patterns

### 1. Repository Pattern (Backend)

```csharp
// FlowBoard.Core/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// FlowBoard.Infrastructure/Data/Repositories/TaskRepository.cs
public class TaskRepository : ITaskRepository
{
    private readonly FlowBoardDbContext _context;
    
    public async Task<IEnumerable<Task>> GetByColumnIdAsync(int columnId)
    {
        return await _context.Tasks
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Position)
            .Include(t => t.Assignee)
            .ToListAsync();
    }
}
```

### 2. Unit of Work Pattern

```csharp
// FlowBoard.Core/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    ITaskRepository Tasks { get; }
    IBoardRepository Boards { get; }
    ITeamRepository Teams { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### 3. HTTP Interceptor (Frontend)

```typescript
// core/interceptors/auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '@app/core/services';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  
  return next(req);
};
```

---

## 🧪 Testing Checklist

**Before committing, ensure:**

- [ ] All tests pass: `pnpm test`
- [ ] No TypeScript errors: `pnpm type-check` (frontend)
- [ ] Code coverage ≥ 80%
- [ ] All new features have tests
- [ ] No `any` types used
- [ ] Proper error handling implemented
- [ ] Documentation updated if needed

---

## 🚀 Development Workflow

### 1. Starting a New Feature

```bash
# 1. Create feature branch
git checkout -b feature/task-comments

# 2. Read relevant documentation
# - docs/DESIGN.md for architecture
# - docs/API_SPEC.md for API details

# 3. Implement with tests
# Backend: Create entity, command, handler, validator, tests
# Frontend: Create component, service, tests

# 4. Run tests
pnpm test

# 5. Commit with conventional commits
git commit -m "feat: add task comments feature"
```

### 2. Debugging

**Backend:**
- Use Serilog for structured logging
- Check logs in console or `logs/` directory
- Use breakpoints in VS Code debugger

**Frontend:**
- Use Angular DevTools for component inspection
- Check Network tab for API calls
- Use Redux DevTools for state (if using NgRx)
- SignalR debugging: Enable `LogLevel.Debug` in HubConnection

---

## 📝 Commit Message Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Build process or auxiliary tool changes

**Examples:**
```bash
feat(board): add drag-and-drop for tasks
fix(auth): resolve token expiration issue
docs(api): update endpoint documentation
test(task): add unit tests for MoveTaskHandler
```

---

## 🎨 Code Style

### Backend (C#)

**Follow Microsoft C# Coding Conventions:**
- Use PascalCase for classes, methods, properties
- Use camelCase for local variables, parameters
- Use `var` when type is obvious
- One class per file
- Async methods should end with `Async`

```csharp
// Good
public async Task<Result<TaskDto>> CreateTaskAsync(CreateTaskCommand command)
{
    var task = new Task { Title = command.Title };
    await _repository.AddAsync(task);
    return Result<TaskDto>.Success(_mapper.Map<TaskDto>(task));
}
```

### Frontend (TypeScript)

**Follow Angular Style Guide:**
- Use camelCase for variables, functions
- Use PascalCase for classes, interfaces, types
- Prefix interfaces with `I` only when necessary (type vs interface decision)
- Use arrow functions for callbacks
- Avoid `!` (non-null assertion) unless absolutely necessary

```typescript
// Good
export interface Task {
  id: number;
  title: string;
  priority: TaskPriority;
}

const handleTaskClick = (taskId: number): void => {
  console.log('Task clicked:', taskId);
};
```

---

## ⚠️ Common Pitfalls to Avoid

### Backend

1. **Don't expose entities directly in API responses**
   - ❌ `return Ok(task);`
   - ✅ `return Ok(_mapper.Map<TaskDto>(task));`

2. **Don't forget concurrency control**
   - Always check `RowVersion` when updating
   - Handle `DbUpdateConcurrencyException`

3. **Don't use `Include` unnecessarily**
   - Only load related data when needed
   - Beware of N+1 query problems

### Frontend

1. **Don't forget to unsubscribe**
   - ❌ `this.service.getData().subscribe(...)`
   - ✅ `this.service.getData().pipe(takeUntil(this.destroy$)).subscribe(...)`

2. **Don't mutate state directly**
   - ❌ `this.tasks.push(newTask)`
   - ✅ `this.tasks = [...this.tasks, newTask]`

3. **Don't use `any` type**
   - Always define proper interfaces/types

---

## 📖 Quick Reference Links

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Angular Documentation](https://angular.io/docs)
- [RxJS Documentation](https://rxjs.dev/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)

---

## 🤝 Need Help?

1. Check `docs/DESIGN.md` for architecture decisions
2. Check `docs/API_SPEC.md` for API details
3. Check `docs/TESTING.md` for testing examples
4. Search existing code for similar patterns
5. Ask the developer for clarification

---

**Remember: Write clean, tested, and well-typed code. Quality over speed!** 🚀
