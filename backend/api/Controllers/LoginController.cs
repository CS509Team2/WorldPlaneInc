using Microsoft.AspNetCore.Mvc;
using model;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{

    //Use this to log in, you might need to change the port: http://127.0.0.1:5237/login/api/login?username=user1&password=1111
    [HttpGet("api/login")]
    public IActionResult Login(string username, string password)
    {
        if (LoginModel.Login(username, password))
        {
            return this.Ok("Welcome " + username + "!");
        }
        else
        {
            return this.Ok("Invalid username or password. Please try again.");
        }
    }
}
