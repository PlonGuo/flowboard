using FlowBoard.Core.Entities;

namespace FlowBoard.Core.Interfaces;

public interface ITeamRepository : IRepository<Team>
{
    Task<Team?> GetByIdWithMembersAsync(int id);
    Task<IEnumerable<Team>> GetByUserIdAsync(int userId);
    Task<bool> IsUserMemberAsync(int teamId, int userId);
    Task<bool> IsUserOwnerAsync(int teamId, int userId);
    Task<Team?> GetByInviteCodeAsync(string inviteCode);
}
