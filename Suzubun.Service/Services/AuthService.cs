using Supabase.Gotrue;
using Suzubun.Service.Interfaces;
using Suzubun.Service.Models;
using Microsoft.Extensions.Options;

namespace Suzubun.Service.Services;

public class AuthService : IAuthService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IEmailService _emailService;

    public AuthService(Supabase.Client supabaseClient, IEmailService emailService)
    {
        _supabaseClient = supabaseClient;
        _emailService = emailService;
    }

    public async Task RegisterAsync(string email, string password, string fullName)
    {
        var session = await _supabaseClient.Auth.SignUp(email, password, new SignUpOptions
        {
            Data = new Dictionary<string, object> { { "full_name", fullName } }
        });

        if (session?.User == null)
        {
            // Handle failure or existing user logic if needed
            // Usually Supabase throws exception or returns null user if config prevents sign up
        }

        // Email confirmation is handled by Supabase automatically (if enabled in dashboard)
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var session = await _supabaseClient.Auth.SignIn(email, password);
        return session.AccessToken;
    }
}
