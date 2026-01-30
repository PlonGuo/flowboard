# FlowBoard Testing Guide

> Comprehensive testing strategy, frameworks, and standards for backend and frontend.

---

## Table of Contents

1. [Testing Philosophy](#1-testing-philosophy)
2. [Backend Testing (.NET/C#)](#2-backend-testing-netc)
3. [Frontend Testing (Angular/TypeScript)](#3-frontend-testing-angulartypescript)
4. [Test Commands](#4-test-commands)
5. [Testing Standards](#5-testing-standards)
6. [CI/CD Integration](#6-cicd-integration)

---

## 1. Testing Philosophy

### 1.1 Testing Pyramid

```
         /\
        /E2E\         <- Few (5%)
       /------\
      /  API   \      <- Some (15%)
     /----------\
    / Integration\    <- More (30%)
   /--------------\
  /  Unit Tests   \   <- Most (50%)
 /------------------\
```

### 1.2 Testing Principles

1. **Test Behavior, Not Implementation** - Tests should verify what the code does, not how it does it
2. **Arrange-Act-Assert (AAA)** - Structure all tests with clear setup, execution, and verification
3. **Fast and Isolated** - Tests should run quickly and independently
4. **Maintainable** - Tests should be easy to read and update
5. **Fail Fast** - Tests should fail immediately when something breaks

### 1.3 Coverage Goals

- **Unit Tests**: 80%+ code coverage
- **Integration Tests**: Critical paths covered
- **E2E Tests**: Happy paths + critical user journeys

---

## 2. Backend Testing (.NET/C#)

### 2.1 Testing Frameworks

**Primary Stack:**
- **xUnit** - Test framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework
- **AutoFixture** - Test data generation
- **Bogus** - Fake data generator
- **WebApplicationFactory** - Integration testing

**Installation:**
```bash
cd src/FlowBoard.Tests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package AutoFixture
dotnet add package AutoFixture.Xunit2
dotnet add package Bogus
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

### 2.2 Unit Testing

#### 2.2.1 Testing Domain Logic

```csharp
// FlowBoard.UnitTests/Core/Entities/TaskTests.cs
using FluentAssertions;
using Xunit;

namespace FlowBoard.UnitTests.Core.Entities;

public class TaskTests
{
    [Fact]
    public void Task_ShouldBeCreated_WithValidData()
    {
        // Arrange
        var title = "Fix login bug";
        var columnId = 1;
        var createdById = 1;

        // Act
        var task = new FlowBoard.Core.Entities.Task
        {
            Title = title,
            ColumnId = columnId,
            CreatedById = createdById,
            Priority = TaskPriority.High
        };

        // Assert
        task.Title.Should().Be(title);
        task.ColumnId.Should().Be(columnId);
        task.CreatedById.Should().Be(createdById);
        task.Priority.Should().Be(TaskPriority.High);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Task_ShouldThrowException_WhenTitleIsInvalid(string invalidTitle)
    {
        // Arrange & Act
        Action act = () => new FlowBoard.Core.Entities.Task
        {
            Title = invalidTitle,
            ColumnId = 1,
            CreatedById = 1
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*title*");
    }

    [Fact]
    public void MoveTask_ShouldUpdatePosition_WhenValid()
    {
        // Arrange
        var task = CreateValidTask();
        var newColumnId = 2;
        var newPosition = 5;

        // Act
        task.Move(newColumnId, newPosition);

        // Assert
        task.ColumnId.Should().Be(newColumnId);
        task.Position.Should().Be(newPosition);
        task.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private FlowBoard.Core.Entities.Task CreateValidTask()
    {
        return new FlowBoard.Core.Entities.Task
        {
            Title = "Test Task",
            ColumnId = 1,
            CreatedById = 1,
            Position = 0
        };
    }
}
```

#### 2.2.2 Testing Application Layer (Handlers)

```csharp
// FlowBoard.UnitTests/Application/Handlers/MoveTaskHandlerTests.cs
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace FlowBoard.UnitTests.Application.Handlers;

public class MoveTaskHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly MoveTaskHandler _handler;
    private readonly Fixture _fixture;

    public MoveTaskHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publisherMock = new Mock<IPublisher>();
        _handler = new MoveTaskHandler(_unitOfWorkMock.Object, _publisherMock.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_ShouldMoveTask_WhenVersionMatches()
    {
        // Arrange
        var task = _fixture.Build<FlowBoard.Core.Entities.Task>()
            .With(t => t.Id, 1)
            .With(t => t.ColumnId, 1)
            .With(t => t.Position, 0)
            .With(t => t.RowVersion, new byte[] { 1, 2, 3 })
            .Create();

        var command = new MoveTaskCommand(
            TaskId: 1,
            ToColumnId: 2,
            ToPosition: 3,
            RowVersion: new byte[] { 1, 2, 3 }
        );

        _unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(1))
            .ReturnsAsync(task);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.ColumnId.Should().Be(2);
        task.Position.Should().Be(3);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.Publish(
            It.IsAny<TaskMovedEvent>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenVersionMismatch()
    {
        // Arrange
        var task = _fixture.Build<FlowBoard.Core.Entities.Task>()
            .With(t => t.Id, 1)
            .With(t => t.RowVersion, new byte[] { 1, 2, 3 })
            .Create();

        var command = new MoveTaskCommand(
            TaskId: 1,
            ToColumnId: 2,
            ToPosition: 3,
            RowVersion: new byte[] { 9, 9, 9 } // Different version
        );

        _unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("modified");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new MoveTaskCommand(1, 2, 3, new byte[] { 1, 2, 3 });

        _unitOfWorkMock.Setup(u => u.Tasks.GetByIdAsync(1))
            .ReturnsAsync((FlowBoard.Core.Entities.Task?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
```

#### 2.2.3 Testing Validators

```csharp
// FlowBoard.UnitTests/Application/Validators/CreateTaskCommandValidatorTests.cs
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace FlowBoard.UnitTests.Application.Validators;

public class CreateTaskCommandValidatorTests
{
    private readonly CreateTaskCommandValidator _validator;

    public CreateTaskCommandValidatorTests()
    {
        _validator = new CreateTaskCommandValidator();
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "",
            ColumnId = 1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenTitleIsTooLong()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = new string('a', 201), // Max is 200
            ColumnId = 1
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Validator_ShouldHaveError_WhenColumnIdIsInvalid()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Valid Title",
            ColumnId = 0
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ColumnId)
            .WithErrorMessage("ColumnId must be greater than 0");
    }

    [Fact]
    public void Validator_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTaskCommand
        {
            Title = "Fix login bug",
            ColumnId = 1,
            Priority = "High"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
```

### 2.3 Integration Testing

#### 2.3.1 API Integration Tests

```csharp
// FlowBoard.IntegrationTests/API/TasksControllerTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FlowBoard.IntegrationTests.API;

public class TasksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TasksControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTask_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var loginResponse = await LoginAsTestUser();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", loginResponse.Token);

        var request = new CreateTaskRequest
        {
            Title = "Integration Test Task",
            ColumnId = 1,
            Priority = "High"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var task = await response.Content.ReadFromJsonAsync<TaskDto>();
        task.Should().NotBeNull();
        task!.Title.Should().Be("Integration Test Task");
        task.Priority.Should().Be("High");
    }

    [Fact]
    public async Task MoveTask_ShouldReturnConflict_WhenVersionMismatch()
    {
        // Arrange
        await LoginAsTestUser();
        var task = await CreateTestTask();

        var moveRequest = new MoveTaskRequest
        {
            ToColumnId = 2,
            ToPosition = 0,
            RowVersion = "INVALID_VERSION"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}/move", moveRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Error.Code.Should().Be("CONFLICT");
    }

    private async Task<LoginResponse> LoginAsTestUser()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "test@example.com",
            Password = "TestPass123!"
        });

        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }
}
```

#### 2.3.2 Database Integration Tests

```csharp
// FlowBoard.IntegrationTests/Infrastructure/TaskRepositoryTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlowBoard.IntegrationTests.Infrastructure;

public class TaskRepositoryTests : IDisposable
{
    private readonly FlowBoardDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FlowBoardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new FlowBoardDbContext(options);
        _repository = new TaskRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTask_WhenExists()
    {
        // Arrange
        var task = new FlowBoard.Core.Entities.Task
        {
            Title = "Test Task",
            ColumnId = 1,
            CreatedById = 1
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetByColumnIdAsync_ShouldReturnOrderedTasks()
    {
        // Arrange
        var tasks = new[]
        {
            new FlowBoard.Core.Entities.Task { Title = "Task 1", ColumnId = 1, Position = 2, CreatedById = 1 },
            new FlowBoard.Core.Entities.Task { Title = "Task 2", ColumnId = 1, Position = 0, CreatedById = 1 },
            new FlowBoard.Core.Entities.Task { Title = "Task 3", ColumnId = 1, Position = 1, CreatedById = 1 }
        };

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByColumnIdAsync(1);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(t => t.Position);
        result.First().Title.Should().Be("Task 2");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### 2.4 SignalR Testing

```csharp
// FlowBoard.IntegrationTests/Hubs/BoardHubTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace FlowBoard.IntegrationTests.Hubs;

public class BoardHubTests : IAsyncLifetime
{
    private HubConnection _connection = null!;
    private readonly List<TaskMovedEvent> _receivedEvents = new();

    public async Task InitializeAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/hubs/board")
            .Build();

        _connection.On<TaskMovedEvent>("TaskMoved", evt =>
        {
            _receivedEvents.Add(evt);
        });

        await _connection.StartAsync();
    }

    [Fact]
    public async Task JoinBoard_ShouldReceiveTaskMovedEvent()
    {
        // Arrange
        await _connection.InvokeAsync("JoinBoard", 1);

        // Act
        // Simulate another user moving a task
        await SimulateTaskMove(taskId: 1, toColumnId: 2);

        await Task.Delay(100); // Wait for event

        // Assert
        _receivedEvents.Should().HaveCount(1);
        _receivedEvents.First().TaskId.Should().Be(1);
        _receivedEvents.First().ToColumnId.Should().Be(2);
    }

    public async Task DisposeAsync()
    {
        await _connection.StopAsync();
        await _connection.DisposeAsync();
    }
}
```

---

## 3. Frontend Testing (Angular/TypeScript)

### 3.1 Testing Frameworks

**Primary Stack:**
- **Jasmine** - Test framework (built-in with Angular)
- **Karma** - Test runner
- **Jest** - Alternative test runner (faster, recommended)
- **@testing-library/angular** - Testing utilities
- **ng-mocks** - Component mocking
- **Cypress** - E2E testing

**Installation:**
```bash
cd flowboard-web

# Jest (recommended)
pnpm add -D jest @types/jest jest-preset-angular
pnpm add -D @testing-library/angular @testing-library/jest-dom
pnpm add -D @testing-library/user-event

# Cypress
pnpm add -D cypress @cypress/angular
```

### 3.2 Unit Testing Components

#### 3.2.1 Testing Component Logic

```typescript
// src/app/features/boards/task-card/task-card.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TaskCardComponent } from './task-card.component';
import { Task, TaskPriority } from '@app/core/models';

describe('TaskCardComponent', () => {
  let component: TaskCardComponent;
  let fixture: ComponentFixture<TaskCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskCardComponent] // Standalone component
    }).compileComponents();

    fixture = TestBed.createComponent(TaskCardComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display task title', () => {
    // Arrange
    const task: Task = {
      id: 1,
      title: 'Fix login bug',
      columnId: 1,
      position: 0,
      priority: TaskPriority.High,
      createdAt: new Date(),
      updatedAt: new Date(),
      rowVersion: 'ABC123'
    };

    // Act
    component.task = task;
    fixture.detectChanges();

    // Assert
    const titleElement = fixture.nativeElement.querySelector('.task-title');
    expect(titleElement.textContent).toContain('Fix login bug');
  });

  it('should emit taskMoved event when dropped', () => {
    // Arrange
    const task: Task = createMockTask();
    component.task = task;

    let emittedEvent: { taskId: number; toColumnId: number } | undefined;
    component.taskMoved.subscribe(event => {
      emittedEvent = event;
    });

    // Act
    component.onDrop({ toColumnId: 2, toPosition: 3 });

    // Assert
    expect(emittedEvent).toEqual({
      taskId: task.id,
      toColumnId: 2,
      toPosition: 3
    });
  });

  it('should show high priority badge when priority is High', () => {
    // Arrange
    component.task = createMockTask({ priority: TaskPriority.High });

    // Act
    fixture.detectChanges();

    // Assert
    const badge = fixture.nativeElement.querySelector('.priority-badge-high');
    expect(badge).toBeTruthy();
  });

  function createMockTask(overrides?: Partial<Task>): Task {
    return {
      id: 1,
      title: 'Test Task',
      columnId: 1,
      position: 0,
      priority: TaskPriority.Medium,
      createdAt: new Date(),
      updatedAt: new Date(),
      rowVersion: 'ABC123',
      ...overrides
    };
  }
});
```

#### 3.2.2 Testing with Testing Library

```typescript
// src/app/features/boards/task-card/task-card.component.spec.ts (with Testing Library)
import { render, screen, fireEvent } from '@testing-library/angular';
import { TaskCardComponent } from './task-card.component';
import userEvent from '@testing-library/user-event';

describe('TaskCardComponent (Testing Library)', () => {
  it('should render task with correct title', async () => {
    // Arrange & Act
    await render(TaskCardComponent, {
      componentInputs: {
        task: {
          id: 1,
          title: 'Fix login bug',
          priority: 'High'
        }
      }
    });

    // Assert
    expect(screen.getByText('Fix login bug')).toBeInTheDocument();
  });

  it('should call onTaskClick when clicked', async () => {
    // Arrange
    const onTaskClick = jest.fn();
    
    await render(TaskCardComponent, {
      componentInputs: {
        task: { id: 1, title: 'Test Task' }
      },
      componentOutputs: {
        taskClicked: onTaskClick
      }
    });

    // Act
    const card = screen.getByRole('article');
    await userEvent.click(card);

    // Assert
    expect(onTaskClick).toHaveBeenCalledWith(1);
  });
});
```

### 3.3 Testing Services

```typescript
// src/app/core/services/board.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BoardService } from './board.service';
import { Board } from '@app/core/models';

describe('BoardService', () => {
  let service: BoardService;
  let httpMock: HttpTestingController;
  const apiUrl = 'https://localhost:5001/api';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [BoardService]
    });

    service = TestBed.inject(BoardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // Verify no outstanding requests
  });

  it('should fetch board by id', (done) => {
    // Arrange
    const mockBoard: Board = {
      id: 1,
      name: 'Sprint 1',
      teamId: 1,
      columns: []
    };

    // Act
    service.getBoard(1).subscribe(board => {
      // Assert
      expect(board).toEqual(mockBoard);
      done();
    });

    // Assert HTTP request
    const req = httpMock.expectOne(`${apiUrl}/boards/1`);
    expect(req.request.method).toBe('GET');
    req.flush({ data: mockBoard });
  });

  it('should handle 404 error', (done) => {
    // Act
    service.getBoard(999).subscribe({
      next: () => fail('Should have failed'),
      error: (error) => {
        // Assert
        expect(error.status).toBe(404);
        done();
      }
    });

    // Assert HTTP request
    const req = httpMock.expectOne(`${apiUrl}/boards/999`);
    req.flush({ error: 'Not found' }, { status: 404, statusText: 'Not Found' });
  });

  it('should create board with correct payload', (done) => {
    // Arrange
    const newBoard = {
      name: 'New Board',
      teamId: 1
    };

    // Act
    service.createBoard(newBoard).subscribe(board => {
      expect(board.id).toBeDefined();
      done();
    });

    // Assert
    const req = httpMock.expectOne(`${apiUrl}/boards`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newBoard);
    req.flush({ data: { ...newBoard, id: 1 } });
  });
});
```

### 3.4 Testing SignalR Integration

```typescript
// src/app/core/services/signalr.service.spec.ts
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { SignalRService } from './signalr.service';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

describe('SignalRService', () => {
  let service: SignalRService;
  let mockHubConnection: jasmine.SpyObj<HubConnection>;

  beforeEach(() => {
    // Create mock HubConnection
    mockHubConnection = jasmine.createSpyObj<HubConnection>(
      'HubConnection',
      ['start', 'stop', 'invoke', 'on', 'off'],
      { state: HubConnectionState.Disconnected }
    );

    mockHubConnection.start.and.returnValue(Promise.resolve());
    mockHubConnection.stop.and.returnValue(Promise.resolve());

    TestBed.configureTestingModule({
      providers: [SignalRService]
    });

    service = TestBed.inject(SignalRService);
    // Inject mock connection
    (service as any).hubConnection = mockHubConnection;
  });

  it('should connect to SignalR hub', fakeAsync(() => {
    // Act
    service.connect();
    tick();

    // Assert
    expect(mockHubConnection.start).toHaveBeenCalled();
  }));

  it('should emit task moved events', fakeAsync(() => {
    // Arrange
    let receivedEvent: any;
    service.onTaskMoved$.subscribe(event => {
      receivedEvent = event;
    });

    // Simulate SignalR receiving event
    const mockEvent = { taskId: 1, toColumnId: 2 };
    const onCallback = mockHubConnection.on.calls.argsFor(0)[1];

    // Act
    onCallback(mockEvent);
    tick();

    // Assert
    expect(receivedEvent).toEqual(mockEvent);
  }));

  it('should join board room', fakeAsync(() => {
    // Act
    service.joinBoard(1);
    tick();

    // Assert
    expect(mockHubConnection.invoke).toHaveBeenCalledWith('JoinBoard', 1);
  }));
});
```

### 3.5 E2E Testing with Cypress

```typescript
// cypress/e2e/board.cy.ts
describe('Board Management', () => {
  beforeEach(() => {
    cy.login('test@example.com', 'TestPass123!');
    cy.visit('/boards/1');
  });

  it('should create a new task', () => {
    // Arrange
    cy.get('[data-testid="add-task-button"]').click();

    // Act
    cy.get('[data-testid="task-title-input"]').type('New Task');
    cy.get('[data-testid="task-priority-select"]').select('High');
    cy.get('[data-testid="save-task-button"]').click();

    // Assert
    cy.contains('New Task').should('be.visible');
    cy.get('[data-testid="priority-badge-high"]').should('exist');
  });

  it('should drag and drop task between columns', () => {
    // Arrange
    const dataTransfer = new DataTransfer();

    // Act
    cy.get('[data-testid="task-card-1"]')
      .trigger('dragstart', { dataTransfer });

    cy.get('[data-testid="column-done"]')
      .trigger('drop', { dataTransfer });

    // Assert
    cy.get('[data-testid="column-done"]')
      .should('contain', 'Task 1');
  });

  it('should show real-time updates from other users', () => {
    // Arrange
    // Simulate another user moving a task via SignalR
    cy.window().then((win) => {
      (win as any).signalRConnection.invoke('SimulateTaskMove', {
        taskId: 1,
        toColumnId: 2
      });
    });

    // Assert
    cy.get('[data-testid="column-2"]')
      .should('contain', 'Task 1');
  });
});
```

---

## 4. Test Commands

### 4.1 Root Package.json Scripts

Add to `package.json` in root:

```json
{
  "scripts": {
    "test": "pnpm run test:back && pnpm run test:front",
    "test:back": "cd src && dotnet test --no-restore --verbosity normal",
    "test:front": "cd flowboard-web && pnpm test",
    "test:back:watch": "cd src && dotnet watch test",
    "test:front:watch": "cd flowboard-web && pnpm test:watch",
    "test:coverage": "pnpm run test:back:coverage && pnpm run test:front:coverage",
    "test:back:coverage": "cd src && dotnet test --collect:\"XPlat Code Coverage\" --results-directory ./coverage",
    "test:front:coverage": "cd flowboard-web && pnpm test:coverage",
    "test:e2e": "cd flowboard-web && pnpm cypress:run"
  }
}
```

### 4.2 Frontend Package.json Scripts

Add to `flowboard-web/package.json`:

```json
{
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "test:coverage": "jest --coverage",
    "test:ci": "jest --ci --coverage --maxWorkers=2",
    "cypress:open": "cypress open",
    "cypress:run": "cypress run"
  }
}
```

### 4.3 Running Tests

```bash
# Run all tests (backend + frontend)
pnpm test

# Run backend tests only
pnpm test:back

# Run frontend tests only
pnpm test:front

# Run tests in watch mode
pnpm test:back:watch
pnpm test:front:watch

# Run with coverage
pnpm test:coverage

# Run E2E tests
pnpm test:e2e
```

---

## 5. Testing Standards

### 5.1 Naming Conventions

**Backend (C#):**
```csharp
// Pattern: MethodName_Should{Expected}_When{Condition}
public void MoveTask_ShouldUpdatePosition_WhenValid()
public void CreateTask_ShouldThrowException_WhenTitleIsEmpty()
public void GetBoard_ShouldReturnNotFound_WhenBoardDoesNotExist()
```

**Frontend (TypeScript):**
```typescript
// Pattern: should {expected} when {condition}
it('should display task title', () => {});
it('should emit event when clicked', () => {});
it('should show error when validation fails', () => {});
```

### 5.2 TypeScript Type Safety

**❌ BAD - Using `any`:**
```typescript
it('should handle response', () => {
  const response: any = { data: { id: 1 } }; // DON'T DO THIS
  expect(response.data.id).toBe(1);
});
```

**✅ GOOD - Proper typing:**
```typescript
interface ApiResponse<T> {
  data: T;
  message?: string;
}

it('should handle response', () => {
  const response: ApiResponse<{ id: number }> = {
    data: { id: 1 }
  };
  expect(response.data.id).toBe(1);
});
```

### 5.3 Test Data Builders

**Backend:**
```csharp
// FlowBoard.Tests/Builders/TaskBuilder.cs
public class TaskBuilder
{
    private string _title = "Default Task";
    private int _columnId = 1;
    private int _createdById = 1;
    private TaskPriority _priority = TaskPriority.Medium;

    public TaskBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TaskBuilder WithPriority(TaskPriority priority)
    {
        _priority = priority;
        return this;
    }

    public FlowBoard.Core.Entities.Task Build()
    {
        return new FlowBoard.Core.Entities.Task
        {
            Title = _title,
            ColumnId = _columnId,
            CreatedById = _createdById,
            Priority = _priority
        };
    }
}

// Usage
var task = new TaskBuilder()
    .WithTitle("Important Task")
    .WithPriority(TaskPriority.High)
    .Build();
```

**Frontend:**
```typescript
// src/app/testing/builders/task.builder.ts
export class TaskBuilder {
  private task: Partial<Task> = {
    id: 1,
    title: 'Default Task',
    columnId: 1,
    position: 0,
    priority: TaskPriority.Medium,
    createdAt: new Date(),
    updatedAt: new Date(),
    rowVersion: 'ABC123'
  };

  withTitle(title: string): this {
    this.task.title = title;
    return this;
  }

  withPriority(priority: TaskPriority): this {
    this.task.priority = priority;
    return this;
  }

  build(): Task {
    return this.task as Task;
  }
}

// Usage
const task = new TaskBuilder()
  .withTitle('Important Task')
  .withPriority(TaskPriority.High)
  .build();
```

---

## 6. CI/CD Integration

### 6.1 GitHub Actions Workflow

```yaml
# .github/workflows/test.yml
name: Run Tests

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
        working-directory: ./src
      
      - name: Build
        run: dotnet build --no-restore
        working-directory: ./src
      
      - name: Test
        run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
        working-directory: ./src
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./src/**/coverage.cobertura.xml

  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install pnpm
        uses: pnpm/action-setup@v2
        with:
          version: 9
      
      - name: Install dependencies
        run: pnpm install
        working-directory: ./flowboard-web
      
      - name: Run tests
        run: pnpm test:ci
        working-directory: ./flowboard-web
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./flowboard-web/coverage/lcov.info
```

---

**This testing guide should be followed for all new features and maintained as the project evolves.**
