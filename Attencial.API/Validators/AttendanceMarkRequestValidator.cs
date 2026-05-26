using Attencial.Shared.Dtos;
using FluentValidation;

namespace Attencial.API.Validators;

public class AttendanceMarkRequestValidator : AbstractValidator<AttendanceMarkRequest>
{
    public AttendanceMarkRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Attendance Session Token is required.");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Base64 scan image is required.");
    }
}
