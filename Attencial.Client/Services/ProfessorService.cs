using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attencial.Shared.Dtos;

namespace Attencial.Client.Services;

public class ProfessorService
{
    private static readonly List<CourseDto> MockClasses = new()
    {
        new CourseDto { Id = 101, Code = "CS-301", Name = "Software Engineering", TotalStudents = 5, TotalLectures = 26 },
        new CourseDto { Id = 103, Code = "CS-303", Name = "Artificial Intelligence", TotalStudents = 4, TotalLectures = 26 }
    };

    private static readonly List<StudentDto> MockStudents = new()
    {
        new StudentDto { Id = 1, Name = "Alice Smith", Email = "alice@example.com", AttendanceRate = 95.5 },
        new StudentDto { Id = 2, Name = "Bob Johnson", Email = "bob@example.com", AttendanceRate = 88.2 },
        new StudentDto { Id = 3, Name = "Charlie Brown", Email = "charlie@example.com", AttendanceRate = 72.0 },
        new StudentDto { Id = 4, Name = "Diana Prince", Email = "diana@example.com", AttendanceRate = 100.0 },
        new StudentDto { Id = 5, Name = "Ethan Hunt", Email = "ethan@example.com", AttendanceRate = 80.0 }
    };

    private static readonly List<AttendanceRecordDto> MockSubmitted = new()
    {
        new AttendanceRecordDto { Id = 1, Date = DateTime.Today.AddDays(-1), CourseId = 101, CourseCode = "CS-301", CourseName = "Software Engineering", Status = "Present", MarkedBy = "Professor" }
    };

    public Task<List<CourseDto>> GetProfessorClassesAsync()
        => Task.FromResult(MockClasses.ToList());

    public Task<List<StudentDto>> GetStudentsInClassAsync(int classId)
        => Task.FromResult(classId == 101 ? MockStudents.ToList() : MockStudents.Take(4).ToList());

    public Task<bool> SubmitAttendanceAsync(AttendanceMarkRequest request)
    {
        foreach (var marking in request.Markings)
        {
            MockSubmitted.Add(new AttendanceRecordDto
            {
                Id = MockSubmitted.Count + 1,
                Date = request.Date,
                CourseId = request.CourseId,
                Status = marking.Status,
                MarkedBy = "Current Professor"
            });
        }
        return Task.FromResult(true);
    }

    public Task<List<AttendanceRecordDto>> GetAttendanceReportAsync(int classId)
        => Task.FromResult(MockSubmitted.Where(r => r.CourseId == classId).ToList());
}