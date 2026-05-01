using Microsoft.AspNetCore.Mvc;
using model;

namespace api.Controllers;

public record LoginRequest(string Username, string Password);
public record UpdateSettingsRequest(
    string CurrentUsername,
    string NewUsername,
    string? CurrentPassword,
    string? NewPassword
);

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{

    private readonly ILoginModel _loginModel;

    public LoginController(ILoginModel loginModel)
    {
        _loginModel = loginModel;
    }

    [HttpPost("api/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {

        if (await _loginModel.LoginAsync(request.Username, request.Password))
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

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password cannot be empty." });
        }

        if (await _loginModel.SignupAsync(request.Username, request.Password))
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

        // Create guest account if it doesn't exist, then log in
        await _loginModel.SignupAsync("guest", "guestPassword");

        if (await _loginModel.LoginAsync("guest", "guestPassword"))
        {
            return Ok(new { message = "Welcome guest!" });
        }

        return StatusCode(500, new { message = "Could not create guest session." });
    }

    [HttpPost("api/settings/update")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsRequest request)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                return StatusCode(500, new { message = "Database connection string is missing." });

            var currentUsername = request.CurrentUsername?.Trim() ?? "";
            var newUsername = request.NewUsername?.Trim() ?? "";
            var currentPassword = request.CurrentPassword?.Trim();
            var newPassword = request.NewPassword?.Trim();

            if (string.IsNullOrWhiteSpace(currentUsername) || string.IsNullOrWhiteSpace(newUsername))
                return BadRequest(new { message = "CurrentUsername and NewUsername are required." });

            var loginModel = new LoginModel(connectionString);

            var (success, errorCode) = await loginModel.UpdateSettingsAsync(
                currentUsername,
                newUsername,
                currentPassword,
                newPassword);

            if (success)
                return Ok(new { message = "Settings updated successfully." });

            if (errorCode == "invalid_credentials")
                return Unauthorized(new { message = "Incorrect password." });

            if (errorCode == "username_taken")
                return Conflict(new { message = "Username is already taken." });

            if (errorCode == "user_not_found")
                return NotFound(new { message = "User was not found." });

            return BadRequest(new { message = "Could not update settings." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Unexpected error while updating settings." });
        }
    }
}
