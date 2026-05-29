using System;

namespace Attencial.Shared.Dtos;

public class AttendanceMarkRequest
{
    public string Token { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty; // base64 string
}
