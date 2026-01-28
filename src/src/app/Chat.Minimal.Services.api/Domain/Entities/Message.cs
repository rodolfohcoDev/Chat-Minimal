using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Minimal.Services.Domain.Entities;

[Table("Messages")]
public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public required string ConversationId { get; set; }

    [Required]
    [MaxLength(20)]
    public required string Role { get; set; } // "user", "assistant", "system"

    [Required]
    [Column(TypeName = "TEXT")] // Para permitir textos longos
    public required string Content { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
