using Attencial.Shared.Dtos;
using FluentValidation;
using System;

namespace Attencial.API.Validators;

public class LeaveRequestCreateRequestValidator : AbstractValidator<LeaveRequestCreateRequest>
{
    public LeaveRequestCreateRequestValidator()
    {
        RuleFor(x => x.LeaveType)
            .NotEmpty().WithMessage("Leave Type is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MinimumLength(10).WithMessage("Reason must be at least 10 characters long.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start Date is required.")
            .NotEqual(default(DateTime)).WithMessage("A valid Start Date is required.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End Date is required.")
            .NotEqual(default(DateTime)).WithMessage("A valid End Date is required.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End Date must be greater than or equal to Start Date.");
    }
}
