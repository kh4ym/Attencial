using Microsoft.EntityFrameworkCore;
using Attencial.API.Models;

namespace Attencial.API.Data;

/// <summary>
/// Primary database context for the Attencial application.
/// Manages users, students, professors, courses, attendance, enrollment, and face vectors.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // One DbSet per table
    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Professor> Professors { get; set; }
    public DbSet<FaceVector> FaceVectors { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<AttendanceSession> AttendanceSessions { get; set; }
    public DbSet<OnlineAttendanceToken> OnlineAttendanceTokens { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<AbuseLog> AbuseLogs { get; set; }
    public DbSet<EnrollmentRequest> EnrollmentRequests { get; set; }
    public DbSet<AttendanceAppeal> AttendanceAppeals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One attendance record per student per session
        modelBuilder.Entity<AttendanceRecord>()
            .HasIndex(ar => new { ar.SessionId, ar.StudentId })
            .IsUnique();

        // One appeal per student per session
        modelBuilder.Entity<AttendanceAppeal>()
            .HasIndex(a => new { a.SessionId, a.StudentId })
            .IsUnique();

        // One enrollment per student per course
        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        // One enrollment *request* per student per course at a time
        modelBuilder.Entity<EnrollmentRequest>()
            .HasIndex(er => new { er.StudentId, er.CourseId })
            .IsUnique();

        // Token string must be unique
        modelBuilder.Entity<OnlineAttendanceToken>()
            .HasIndex(t => t.Token)
            .IsUnique();

        // One student record per user
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.UserId)
            .IsUnique();

        // One professor record per user
        modelBuilder.Entity<Professor>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        // Email must be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
