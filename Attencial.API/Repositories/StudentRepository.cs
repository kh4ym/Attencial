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
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students
            .Include(s => s.User)
            .ToListAsync();
    }

    public async Task AddAsync(Student student)
    {
        _context.Students.Add(student);
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
