using FlowBoard.Core.Entities;

namespace FlowBoard.Core.Interfaces;

public interface IBoardRepository : IRepository<Board>
{
    Task<Board?> GetByIdWithColumnsAsync(int id);
    Task<Board?> GetByIdWithColumnsAndTasksAsync(int id);
    Task<IEnumerable<Board>> GetByTeamIdAsync(int teamId);
    Task<IEnumerable<Board>> GetByUserIdAsync(int userId);
}
