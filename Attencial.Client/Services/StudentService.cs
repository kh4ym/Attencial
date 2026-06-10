using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attencial.Shared.Dtos;

namespace Attencial.Client.Services;

public class StudentService
{
    private static readonly List<CourseDto> MockCourses = new()
    {
        new CourseDto { Id = 101, Code = "CS-301", Name = "Software Engineering", ProfessorName = "Dr. Alan Turing", ProfessorEmail = "turing@uni.edu", AttendanceRate = 92.3, TotalLectures = 26, AttendedLectures = 24 },
        new CourseDto { Id = 102, Code = "CS-302", Name = "Database Systems", ProfessorName = "Dr. Edgar Codd", ProfessorEmail = "codd@uni.edu", AttendanceRate = 84.6, TotalLectures = 26, AttendedLectures = 22 },
        new CourseDto { Id = 103, Code = "CS-303", Name = "Artificial Intelligence", ProfessorName = "Dr. Marvin Minsky", ProfessorEmail = "minsky@uni.edu", AttendanceRate = 76.9, TotalLectures = 26, AttendedLectures = 20 },
        new CourseDto { Id = 104, Code = "CS-304", Name = "Computer Networks", ProfessorName = "Dr. Vint Cerf", ProfessorEmail = "cerf@uni.edu", AttendanceRate = 100.0, TotalLectures = 26, AttendedLectures = 26 }
    };

    private static readonly List<AttendanceRecordDto> MockRecords = new()
    {
        new AttendanceRecordDto { Id = 1, Date = DateTime.Today.AddDays(-1), CourseId = 101, CourseCode = "CS-301", CourseName = "Software Engineering", Status = "Present", MarkedBy = "Dr. Alan Turing" },
        new AttendanceRecordDto { Id = 2, Date = DateTime.Today.AddDays(-1), CourseId = 102, CourseCode = "CS-302", CourseName = "Database Systems", Status = "Present", MarkedBy = "Dr. Edgar Codd" },
        new AttendanceRecordDto { Id = 3, Date = DateTime.Today.AddDays(-2), CourseId = 103, CourseCode = "CS-303", CourseName = "Artificial Intelligence", Status = "Absent", MarkedBy = "Dr. Marvin Minsky" },
        new AttendanceRecordDto { Id = 4, Date = DateTime.Today.AddDays(-2), CourseId = 104, CourseCode = "CS-304", CourseName = "Computer Networks", Status = "Present", MarkedBy = "Dr. Vint Cerf" },
        new AttendanceRecordDto { Id = 5, Date = DateTime.Today.AddDays(-3), CourseId = 101, CourseCode = "CS-301", CourseName = "Software Engineering", Status = "Present", MarkedBy = "Dr. Alan Turing" },
        new AttendanceRecordDto { Id = 6, Date = DateTime.Today.AddDays(-3), CourseId = 102, CourseCode = "CS-302", CourseName = "Database Systems", Status = "Late", MarkedBy = "Dr. Edgar Codd" },
        new AttendanceRecordDto { Id = 7, Date = DateTime.Today.AddDays(-4), CourseId = 103, CourseCode = "CS-303", CourseName = "Artificial Intelligence", Status = "Present", MarkedBy = "Dr. Marvin Minsky" },
        new AttendanceRecordDto { Id = 8, Date = DateTime.Today.AddDays(-4), CourseId = 104, CourseCode = "CS-304", CourseName = "Computer Networks", Status = "Present", MarkedBy = "Dr. Vint Cerf" },
    };

    private static readonly List<LeaveRequestDto> MockLeaves = new()
    {
        new LeaveRequestDto { Id = 1, Date = DateTime.Today.AddDays(2), CourseName = "Artificial Intelligence", Type = "Sick", Reason = "Doctor's appointment", Status = "Approved" },
        new LeaveRequestDto { Id = 2, Date = DateTime.Today.AddDays(5), CourseName = "Software Engineering", Type = "Personal", Reason = "Family wedding", Status = "Pending" }
    };

    public Task<List<CourseDto>> GetStudentCoursesAsync()
        => Task.FromResult(MockCourses.ToList());

    public Task<List<AttendanceRecordDto>> GetStudentAttendanceAsync()
        => Task.FromResult(MockRecords.OrderByDescending(r => r.Date).ToList());

    public Task<List<LeaveRequestDto>> GetLeaveRequestsAsync()
        => Task.FromResult(MockLeaves.OrderByDescending(r => r.Date).ToList());

    public Task<bool> SubmitLeaveRequestAsync(LeaveRequestDto request)
    {
        request.Id = MockLeaves.Count + 1;
        request.Status = "Pending";
        MockLeaves.Add(request);
        return Task.FromResult(true);
    }
}