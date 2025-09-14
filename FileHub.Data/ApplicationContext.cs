using FileHub.Application.Abstractions.Data;
using FileHub.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FileHub.Data;

internal class ApplicationContext(DbContextOptions<ApplicationContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>(options), IApplicationContext
{
    public DbSet<StoredFile> StoredFiles { get; init; }

    public DbSet<Tag> Tags { get; init; }

    public DbSet<Category> Categories { get; init; }

    public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();

    public Task MigrateAsync() => Database.MigrateAsync();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique();

        builder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        base.OnModelCreating(builder);
    }
}

internal class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
    public ApplicationContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "FileHub.Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("DefaultConnection"));

        return new ApplicationContext(optionsBuilder.Options);
    }
}
