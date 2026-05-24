using System.Collections.Generic;

namespace Attencial.Shared.Dtos;

public record EnrollRequest
{
    public List<string> Images { get; init; } = new();
}
