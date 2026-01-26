namespace Khadamat.Shared.Interfaces;

public interface IBiometricService
{
    Task<bool> IsAvailableAsync();
    Task<bool> AuthenticateAsync(string reason);
}
