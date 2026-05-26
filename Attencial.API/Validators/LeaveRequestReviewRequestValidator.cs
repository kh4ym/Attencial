using Attencial.Shared.Dtos;
using FluentValidation;

namespace Attencial.API.Validators;

public class LeaveRequestReviewRequestValidator : AbstractValidator<LeaveRequestReviewRequest>
{
    public LeaveRequestReviewRequestValidator()
    {
        RuleFor(x => x.AdminNote)
            .NotEmpty().WithMessage("Admin Note is required.")
            .MinimumLength(10).WithMessage("Admin Note must be at least 10 characters long.");
    }
}
