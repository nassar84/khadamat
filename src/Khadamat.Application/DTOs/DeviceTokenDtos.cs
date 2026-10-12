namespace Khadamat.Application.DTOs;

public class RegisterDeviceRequest
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = "android";
    public string? DeviceId { get; set; }
    public string? AppVersion { get; set; }
}
