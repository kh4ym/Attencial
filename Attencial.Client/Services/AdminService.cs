using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attencial.Shared.Dtos;

namespace Attencial.Client.Services;

public class AdminService
{
    private static readonly List<UserDto> MockUsers = new()
    {
        new UserDto { Id = 1, Name = "Alice Smith", Email = "alice@example.com", Role = "Student" },
        new UserDto { Id = 2, Name = "Bob Johnson", Email = "bob@example.com", Role = "Student" },
        new UserDto { Id = 3, Name = "Dr. Alan Turing", Email = "turing@example.com", Role = "Professor" },
        new UserDto { Id = 4, Name = "Dr. Edgar Codd", Email = "codd@example.com", Role = "Professor" },
        new UserDto { Id = 5, Name = "System Admin", Email = "admin@example.com", Role = "Admin" }
    };

    private static readonly List<CourseDto> MockCourses = new()
    {
        new CourseDto { Id = 101, Code = "CS-301", Name = "Software Engineering", ProfessorName = "Dr. Alan Turing", TotalStudents = 5, AttendanceRate = 92.3 },
        new CourseDto { Id = 102, Code = "CS-302", Name = "Database Systems", ProfessorName = "Dr. Edgar Codd", TotalStudents = 4, AttendanceRate = 84.6 },
        new CourseDto { Id = 103, Code = "CS-303", Name = "Artificial Intelligence", ProfessorName = "Dr. Marvin Minsky", TotalStudents = 4, AttendanceRate = 76.9 },
        new CourseDto { Id = 104, Code = "CS-304", Name = "Computer Networks", ProfessorName = "Dr. Vint Cerf", TotalStudents = 5, AttendanceRate = 100.0 }
    };

    public Task<List<UserDto>> GetUsersAsync()
        => Task.FromResult(MockUsers.ToList());

    public Task<bool> CreateUserAsync(UserDto user)
    {
        user.Id = MockUsers.Max(u => u.Id) + 1;
        MockUsers.Add(user);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        var user = MockUsers.FirstOrDefault(u => u.Id == id);
        if (user != null) { MockUsers.Remove(user); return Task.FromResult(true); }
        return Task.FromResult(false);
    }

    public Task<List<CourseDto>> GetCoursesAsync()
        => Task.FromResult(MockCourses.ToList());

    public Task<bool> CreateCourseAsync(CourseDto course)
    {
        course.Id = MockCourses.Max(c => c.Id) + 1;
        MockCourses.Add(course);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteCourseAsync(int id)
    {
        var course = MockCourses.FirstOrDefault(c => c.Id == id);
        if (course != null) { MockCourses.Remove(course); return Task.FromResult(true); }
        return Task.FromResult(false);
    }
}