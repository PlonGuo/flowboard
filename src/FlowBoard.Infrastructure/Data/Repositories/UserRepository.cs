using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for User entity with authentication-specific queries.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(FlowBoardDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
