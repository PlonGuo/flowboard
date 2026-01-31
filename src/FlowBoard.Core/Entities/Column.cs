namespace FlowBoard.Core.Entities;

public class Column
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int BoardId { get; set; }
    public int Position { get; set; }
    public int? WipLimit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Board Board { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
