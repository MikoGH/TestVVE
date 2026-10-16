using Microsoft.EntityFrameworkCore;

namespace TestVVE;

public class TaskDbContext : DbContext
{
    public DbSet<TaskItem> TaskItems { get; set; }

    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("task_items");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).HasColumnName("id").IsRequired().ValueGeneratedNever();
            entity.Property(t => t.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(t => t.IsCompleted).HasColumnName("isCompleted").IsRequired();
            entity.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(t => t.CompletedAt).HasColumnName("completed_at");
            entity.Property(t => t.Priority).HasColumnName("priority").IsRequired();

            entity.Property(t => t.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });
    }
}
