using Microsoft.EntityFrameworkCore;
using Attencial.API.Data;
using Attencial.API.Models;

namespace Attencial.API.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Professor)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Professor)
            .ToListAsync();
    }

    public async Task<List<Course>> GetByProfessorIdAsync(int professorId)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.ProfessorId == professorId)
            .Include(c => c.Professor)
            .ToListAsync();
    }

    public Task AddAsync(Course course)
    {
        _context.Courses.Add(course);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
