using Microsoft.EntityFrameworkCore;
using Attencial.API.Data;
using Attencial.API.Models;

namespace Attencial.API.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students
            .AsNoTracking()
            .Include(s => s.User)
            .ToListAsync();
    }

    public Task AddAsync(Student student)
    {
        _context.Students.Add(student);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
