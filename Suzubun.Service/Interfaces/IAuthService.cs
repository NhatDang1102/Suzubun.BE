namespace Suzubun.Service.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(string email, string password, string fullName);
    Task<string> LoginAsync(string email, string password);
}
