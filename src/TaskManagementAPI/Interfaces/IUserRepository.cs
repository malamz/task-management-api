
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Interfaces;

/// <summary>User-specific query methods on top of the generic <see cref="IRepository{TEntity}"/>.</summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>Look up a user by their unique email address.</summary>
    Task<User?> FindByEmailAsync(string email);
}
