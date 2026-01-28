using Chat.Minimal.Services.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat.Minimal.Services.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Message Entity
        builder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId); // Improve history lookup performance
            entity.Property(e => e.Content).HasColumnType("TEXT"); // Ensure large text support
        });

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasMany(e => e.ApiKeys)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ApiKey entity
        builder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(e => e.Key)
                .IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.UserId)
                .IsRequired();
        });
    }
}
