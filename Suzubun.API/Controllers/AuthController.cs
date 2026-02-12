using Microsoft.AspNetCore.Mvc;
using Suzubun.Service.Interfaces;

namespace Suzubun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(request.Email, request.Password, request.FullName);
            return Ok(new { Message = "Registration successful. Please check your email for confirmation link." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(new { Token = token });
        }
        catch (Exception)
        {
            return Unauthorized(new { Error = "Invalid credentials or email not confirmed." });
        }
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
