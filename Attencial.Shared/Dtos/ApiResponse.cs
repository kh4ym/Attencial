using System;


namespace Attencial.Shared.Dtos
{
    public record ApiResponse<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
    }
}
