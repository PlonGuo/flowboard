namespace FlowBoard.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<Team> OwnedTeams { get; set; } = new List<Team>();
    public ICollection<Board> CreatedBoards { get; set; } = new List<Board>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Canvas> CreatedCanvases { get; set; } = new List<Canvas>();
    public ICollection<AIChatHistory> ChatHistory { get; set; } = new List<AIChatHistory>();
}
