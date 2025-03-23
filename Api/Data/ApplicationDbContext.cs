using Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<StoredFile> StoredFiles { get; init; }
    public DbSet<Tag> Tags { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tag>().HasIndex(t => t.Name).IsUnique();

        modelBuilder
            .Entity<StoredFile>()
            .HasMany(f => f.Tags)
            .WithMany(t => t.StoredFiles)
            .UsingEntity(j => j.ToTable("StoredFileTags"));
    }
}
