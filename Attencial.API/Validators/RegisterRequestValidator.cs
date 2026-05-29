using Attencial.Shared.Dtos;
using FluentValidation;
using System;

namespace Attencial.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => role.Equals("Student", StringComparison.OrdinalIgnoreCase) || 
                          role.Equals("Professor", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Role must be either 'Student' or 'Professor'.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name is required for Students.")
            .MinimumLength(3).WithMessage("Full Name must be at least 3 characters long.")
            .When(x => x.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.RollNumber)
            .NotEmpty().WithMessage("Roll Number is required for Students.")
            .When(x => x.Role.Equals("Student", StringComparison.OrdinalIgnoreCase));
    }
}
