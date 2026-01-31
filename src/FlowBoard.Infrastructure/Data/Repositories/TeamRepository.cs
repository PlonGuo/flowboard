using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Data.Repositories;

public class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(FlowBoardDbContext context) : base(context)
    {
    }

    public async Task<Team?> GetByIdWithMembersAsync(int id)
    {
        return await DbSet
            .Include(t => t.Owner)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Team>> GetByUserIdAsync(int userId)
    {
        return await DbSet
            .Where(t => t.OwnerId == userId || t.Members.Any(m => m.UserId == userId))
            .OrderByDescending(t => t.UpdatedAt)
            .Include(t => t.Owner)
            .Include(t => t.Members)
            .ToListAsync();
    }

    public async Task<bool> IsUserMemberAsync(int teamId, int userId)
    {
        return await DbSet
            .Where(t => t.Id == teamId)
            .AnyAsync(t => t.OwnerId == userId || t.Members.Any(m => m.UserId == userId));
    }

    public async Task<bool> IsUserOwnerAsync(int teamId, int userId)
    {
        return await DbSet
            .AnyAsync(t => t.Id == teamId && t.OwnerId == userId);
    }
}
