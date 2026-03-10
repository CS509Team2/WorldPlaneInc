using Microsoft.AspNetCore.Mvc;
using model;

namespace api.Controllers;

public record LoginRequest(string Username, string Password);

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    [HttpPost("api/login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (LoginModel.Login(request.Username, request.Password))
        {
            return Ok(new { message = "Welcome " + request.Username + "!" });
        }
        else
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }
    }

    [HttpPost("api/signup")]
    public IActionResult Signup([FromBody] LoginRequest request)
    {
        if (LoginModel.Signup(request.Username, request.Password))
        {
            return Ok(new { message = "Account created successfully." });
        }
        else
        {
            return Conflict(new { message = "Username is already taken." });
        }
    }
}
