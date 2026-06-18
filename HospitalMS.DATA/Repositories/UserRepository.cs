using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
    Task<User> AddAsync(User user);
    void Update(User user);
    void Delete(User user);
    Task<bool> EmailExistsAsync(string email);
}

public class UserRepository : IUserRepository
{
    private readonly HospitalDbContext _context;
    public UserRepository(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(Models.Enums.UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        return user;
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public void Delete(User user)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        _context.Users.Update(user);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}