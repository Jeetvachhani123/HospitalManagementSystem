using HospitalMS.Models.Entities;
using HospitalMS.Models.Enums;

namespace HospitalMS.BL.Interfaces.Repositories;

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