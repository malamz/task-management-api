using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Interfaces;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }
}
