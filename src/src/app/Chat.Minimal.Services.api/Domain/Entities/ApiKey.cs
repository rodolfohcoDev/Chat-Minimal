namespace Chat.Minimal.Services.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign key
    public string UserId { get; set; } = string.Empty;

    // Navigation property
    public virtual User? User { get; set; }
}
