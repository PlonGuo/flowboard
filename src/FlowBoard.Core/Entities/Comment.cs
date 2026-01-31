namespace FlowBoard.Core.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEdited { get; set; } = false;

    // Navigation properties
    public TaskItem Task { get; set; } = null!;
    public User Author { get; set; } = null!;
}
