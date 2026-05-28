using Attencial.API.Models;

namespace Attencial.API.Repositories;

/// <summary>
/// Data access for Student entities with associated User navigation.
/// </summary>
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByUserIdAsync(int userId);
    Task<List<Student>> GetAllAsync();
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task SaveChangesAsync();
}
