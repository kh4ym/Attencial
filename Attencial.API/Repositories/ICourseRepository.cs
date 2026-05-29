using Attencial.API.Models;

namespace Attencial.API.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int id);
    Task<List<Course>> GetAllAsync();
    Task<List<Course>> GetByProfessorIdAsync(int professorId);
    Task AddAsync(Course course);
    Task SaveChangesAsync();
}
