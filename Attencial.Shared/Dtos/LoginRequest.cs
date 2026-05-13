using System;
using System.Collections.Generic;
using System.Text;

namespace Attencial.Shared.Dtos;

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
