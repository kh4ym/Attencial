using System;
using System.Collections.Generic;
using System.Text;

namespace Attencial.Shared.Dtos;

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}