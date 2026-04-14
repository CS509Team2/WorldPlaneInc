using Microsoft.AspNetCore.Mvc;
using model;

namespace api.Controllers;

public record LoginRequest(string Username, string Password);

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public LoginController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("api/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return StatusCode(500, "Database connection string is missing.");

        var loginModel = new LoginModel(connectionString);

        if (await loginModel.LoginAsync(request.Username, request.Password))
        {
            return Ok(new { message = "Welcome " + request.Username + "!" });
        }
        else
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }
    }

    [HttpPost("api/signup")]
    public async Task<IActionResult> Signup([FromBody] LoginRequest request)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return StatusCode(500, "Database connection string is missing.");

        var loginModel = new LoginModel(connectionString);

        if (await loginModel.SignupAsync(request.Username, request.Password))
        {
            return Ok(new { message = "Account created successfully." });
        }
        else
        {
            return Conflict(new { message = "Username is already taken." });
        }
    }

    [HttpPost("api/guest")]
    public async Task<IActionResult> GuestLogin()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            return StatusCode(500, "Database connection string is missing.");

        var loginModel = new LoginModel(connectionString);

        // Create guest account if it doesn't exist, then log in
        await loginModel.SignupAsync("guest", "guestPassword");

        if (await loginModel.LoginAsync("guest", "guestPassword"))
        {
            return Ok(new { message = "Welcome guest!" });
        }

        return StatusCode(500, new { message = "Could not create guest session." });
    }
}
