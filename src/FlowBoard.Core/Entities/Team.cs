namespace FlowBoard.Core.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Canvas> Canvases { get; set; } = new List<Canvas>();
}
