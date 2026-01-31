using FlowBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Data;

public class FlowBoardDbContext : DbContext
{
    public FlowBoardDbContext(DbContextOptions<FlowBoardDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Column> Columns => Set<Column>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Canvas> Canvases => Set<Canvas>();
    public DbSet<CanvasData> CanvasData => Set<CanvasData>();
    public DbSet<CanvasOperation> CanvasOperations => Set<CanvasOperation>();
    public DbSet<AIChatHistory> AIChatHistory => Set<AIChatHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlowBoardDbContext).Assembly);
    }
}
