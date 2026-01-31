using FlowBoard.Core.Entities;

namespace FlowBoard.Core.Interfaces;

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByColumnIdAsync(int columnId);
    Task<IEnumerable<TaskItem>> GetByBoardIdAsync(int boardId);
    Task<IEnumerable<TaskItem>> GetByAssigneeIdAsync(int assigneeId);
    Task<TaskItem?> GetByIdWithCommentsAsync(int id);
    Task<int> GetMaxPositionInColumnAsync(int columnId);
}
