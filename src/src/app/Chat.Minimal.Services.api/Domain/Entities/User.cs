using Microsoft.AspNetCore.Identity;

namespace Chat.Minimal.Services.Domain.Entities;

public class User : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
