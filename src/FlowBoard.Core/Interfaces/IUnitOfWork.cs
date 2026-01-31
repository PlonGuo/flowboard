namespace FlowBoard.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITaskRepository Tasks { get; }
    IBoardRepository Boards { get; }
    IUserRepository Users { get; }
    ITeamRepository Teams { get; }
    IRepository<Entities.TeamMember> TeamMembers { get; }
    IRepository<Entities.Column> Columns { get; }
    IRepository<Entities.Comment> Comments { get; }
    IRepository<Entities.Notification> Notifications { get; }
    IRepository<Entities.Canvas> Canvases { get; }
    IRepository<Entities.CanvasData> CanvasData { get; }
    IRepository<Entities.ActivityLog> ActivityLogs { get; }
    IRepository<Entities.AIChatHistory> AIChatHistory { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
