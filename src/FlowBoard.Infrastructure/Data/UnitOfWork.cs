using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using FlowBoard.Infrastructure.Data.Repositories;

namespace FlowBoard.Infrastructure.Data;

/// <summary>
/// Unit of Work implementation for managing transactions across repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly FlowBoardDbContext _context;
    private bool _disposed;

    // Lazy-loaded repositories
    private ITaskRepository? _tasks;
    private IBoardRepository? _boards;
    private IUserRepository? _users;
    private ITeamRepository? _teams;
    private IRepository<TeamMember>? _teamMembers;
    private IRepository<Column>? _columns;
    private IRepository<Comment>? _comments;
    private IRepository<Notification>? _notifications;
    private IRepository<Canvas>? _canvases;
    private IRepository<CanvasData>? _canvasData;
    private IRepository<ActivityLog>? _activityLogs;
    private IRepository<AIChatHistory>? _aiChatHistory;

    public UnitOfWork(FlowBoardDbContext context)
    {
        _context = context;
    }

    public ITaskRepository Tasks =>
        _tasks ??= new TaskRepository(_context);

    public IBoardRepository Boards =>
        _boards ??= new BoardRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public ITeamRepository Teams =>
        _teams ??= new TeamRepository(_context);

    public IRepository<TeamMember> TeamMembers =>
        _teamMembers ??= new Repository<TeamMember>(_context);

    public IRepository<Column> Columns =>
        _columns ??= new Repository<Column>(_context);

    public IRepository<Comment> Comments =>
        _comments ??= new Repository<Comment>(_context);

    public IRepository<Notification> Notifications =>
        _notifications ??= new Repository<Notification>(_context);

    public IRepository<Canvas> Canvases =>
        _canvases ??= new Repository<Canvas>(_context);

    public IRepository<CanvasData> CanvasData =>
        _canvasData ??= new Repository<CanvasData>(_context);

    public IRepository<ActivityLog> ActivityLogs =>
        _activityLogs ??= new Repository<ActivityLog>(_context);

    public IRepository<AIChatHistory> AIChatHistory =>
        _aiChatHistory ??= new Repository<AIChatHistory>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
