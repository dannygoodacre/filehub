using FileHub.Core.Entities;

namespace FileHub.Application.Abstractions.Data.Repositories;

/// <summary>
/// A repository for system users.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieve a user by their ID.
    /// </summary>
    Task<User?> GetByIdAsync(int id);
}
