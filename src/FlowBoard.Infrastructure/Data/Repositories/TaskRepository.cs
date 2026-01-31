using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Data.Repositories;

/// <summary>
/// Task-specific repository with additional query methods.
/// </summary>
public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(FlowBoardDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TaskItem>> GetByColumnIdAsync(int columnId)
    {
        return await DbSet
            .Where(t => t.ColumnId == columnId)
            .OrderBy(t => t.Position)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByBoardIdAsync(int boardId)
    {
        return await DbSet
            .Where(t => t.Column.BoardId == boardId)
            .OrderBy(t => t.Column.Position)
            .ThenBy(t => t.Position)
            .Include(t => t.Column)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByAssigneeIdAsync(int assigneeId)
    {
        return await DbSet
            .Where(t => t.AssigneeId == assigneeId)
            .OrderByDescending(t => t.UpdatedAt)
            .Include(t => t.Column)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdWithCommentsAsync(int id)
    {
        return await DbSet
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.Assignee)
            .Include(t => t.CreatedBy)
            .Include(t => t.Column)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> GetMaxPositionInColumnAsync(int columnId)
    {
        var maxPosition = await DbSet
            .Where(t => t.ColumnId == columnId)
            .MaxAsync(t => (int?)t.Position);

        return maxPosition ?? -1;
    }
}
