using Microsoft.AspNetCore.Mvc;
using model;

namespace api.Controllers;

public record LoginRequest(string Username, string Password);

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
}
