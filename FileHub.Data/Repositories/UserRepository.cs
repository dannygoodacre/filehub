using FileHub.Application.Abstractions.Data.Repositories;
using FileHub.Core.Entities;
using FileHub.Data.Extensions;

namespace FileHub.Data.Repositories;

internal class UserRepository(ApplicationContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(int id) => (await context.Users.FindAsync(id))?.ToUser();
}
