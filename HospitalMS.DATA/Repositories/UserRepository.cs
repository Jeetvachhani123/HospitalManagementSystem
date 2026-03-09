using HospitalMS.BL.Interfaces.Repositories;
using HospitalMS.DATA.Context;
using HospitalMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.DATA.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HospitalDbContext _context;
    public UserRepository(HospitalDbContext context)
    {
        _context = context;
    }

    // get user by id
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // get user by email
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    // get all users
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .ToListAsync();
    }

    // get users by role
    public async Task<IEnumerable<User>> GetByRoleAsync(Models.Enums.UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .ToListAsync();
    }

    // add user
    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        return user;
    }

    // update user
    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    // soft delete user
    public void Delete(User user)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        _context.Users.Update(user);
    }

    // check email exists
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}