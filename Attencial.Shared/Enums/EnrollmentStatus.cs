namespace Attencial.Shared.Enums;

public enum EnrollmentStatus
{
    Pending,    // Not yet enrolled — no faces submitted
    Trained,    // Ready for attendance ✅
    Failed      // Training failed — needs retry
}