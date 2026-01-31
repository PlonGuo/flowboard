using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Data.Repositories;

/// <summary>
/// Board-specific repository with additional query methods.
/// </summary>
public class BoardRepository : Repository<Board>, IBoardRepository
{
    public BoardRepository(FlowBoardDbContext context) : base(context)
    {
    }

    public async Task<Board?> GetByIdWithColumnsAsync(int id)
    {
        return await DbSet
            .Include(b => b.Columns.OrderBy(c => c.Position))
            .Include(b => b.Team)
            .Include(b => b.CreatedBy)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Board?> GetByIdWithColumnsAndTasksAsync(int id)
    {
        return await DbSet
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Tasks.OrderBy(t => t.Position))
                    .ThenInclude(t => t.Assignee)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Tasks)
                    .ThenInclude(t => t.CreatedBy)
            .Include(b => b.Team)
            .Include(b => b.CreatedBy)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Board>> GetByTeamIdAsync(int teamId)
    {
        return await DbSet
            .Where(b => b.TeamId == teamId)
            .OrderByDescending(b => b.UpdatedAt)
            .Include(b => b.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<Board>> GetByUserIdAsync(int userId)
    {
        return await DbSet
            .Where(b => b.CreatedById == userId ||
                        b.Team.Members.Any(m => m.UserId == userId))
            .OrderByDescending(b => b.UpdatedAt)
            .Include(b => b.Team)
            .Include(b => b.CreatedBy)
            .ToListAsync();
    }
}
