namespace FileHub.Application.Abstractions.Data;

public interface IApplicationContext
{
    /// <summary>
    /// Save all changes made to the context.
    /// </summary>
    /// <returns>The number of state entities written to the database.</returns>
    public Task<int> SaveChangesAsync();

    /// <summary>
    /// Asynchronously applies any pending migrations for the context to the database.
    /// </summary>
    public Task MigrateAsync();
}
