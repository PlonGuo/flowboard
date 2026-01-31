namespace FlowBoard.Core.Entities;

public class AIChatHistory
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? BoardId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Board? Board { get; set; }
}
