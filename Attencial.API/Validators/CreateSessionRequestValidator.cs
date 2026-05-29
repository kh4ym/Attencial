using Attencial.Shared.Dtos;
using FluentValidation;
using System.Linq;

namespace Attencial.API.Validators;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("A valid Course ID is required.");

        RuleFor(x => x.ExpiryMinutes)
            .Must(m => new[] { 5, 10, 15, 30 }.Contains(m))
            .WithMessage("Expiry minutes must be either 5, 10, 15, or 30.");
    }
}
