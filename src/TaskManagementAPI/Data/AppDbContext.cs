using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Data;

/// <summary>
/// Application-level DbContext.  All entity configuration is done here with
/// the Fluent API so the domain models stay clean.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Models.Task> Tasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).HasConversion<string>(); // store as "User"/"Admin"
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── Task ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Models.Task>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Id).ValueGeneratedOnAdd();
            entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(2000);
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Priority).HasConversion<string>();
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(t => t.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // FK relationship: Task.AssignedUserId → User.Id
            entity.HasOne(t => t.AssignedUser)
                  .WithMany(u => u.AssignedTasks)
                  .HasForeignKey(t => t.AssignedUserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
