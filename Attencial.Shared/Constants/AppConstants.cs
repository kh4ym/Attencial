namespace Attencial.Shared.Constants;

/// <summary>
/// Application-wide constants for roles, statuses, and other magic strings.
/// </summary>
public static class AppConstants
{
    public static class Roles
    {
        public const string Student = "Student";
        public const string Professor = "Professor";
        public const string Admin = "Admin";
    }

    public static class EnrollmentStatuses
    {
        public const string Pending = "Pending";
        public const string Trained = "Trained";
        public const string Failed = "Failed";
    }

    public static class RequestStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

    public static class FacultyAttendanceStatuses
    {
        public const string CheckedIn = "CheckedIn";
        public const string FullDay = "FullDay";
        public const string ShortShift = "ShortShift";
    }

    public static class AbuseTypes
    {
        public const string NotEnrolledInCourse = "NotEnrolledInCourse";
        public const string BruteForceLoginLockout = "Brute Force Login Lockout";
    }

    public static class AttendanceStatuses
    {
        public const string Green = "Green";
        public const string Yellow = "Yellow";
        public const string Red = "Red";
    }
}
