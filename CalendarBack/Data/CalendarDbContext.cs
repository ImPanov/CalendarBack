using Microsoft.EntityFrameworkCore;
using CalendarBack.Models;

namespace CalendarBack.Data;

public class CalendarDbContext : DbContext
{
    public CalendarDbContext(DbContextOptions<CalendarDbContext> options)
        : base(options)
    {
    }

    public DbSet<CalendarEntry> CalendarEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarEntry>(entity =>
        {
            entity.ToTable("CalendarEntries");
            
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.ReminderDateTime)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone");

            entity.HasIndex(e => e.ReminderDateTime);
            entity.HasIndex(e => e.Title);
        });

        base.OnModelCreating(modelBuilder);
    }
}
