using Attencial.API.Models;

namespace Attencial.API.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByUserIdAsync(int userId);
    Task<List<Student>> GetAllAsync();
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task SaveChangesAsync();
}
