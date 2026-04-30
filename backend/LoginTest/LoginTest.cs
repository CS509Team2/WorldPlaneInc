using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using model;
using api.Controllers;

namespace LoginTest;

public class LoginTest
{

    private readonly Mock<ILoginModel> _mockLoginModel;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly LoginController _controller;

    public LoginTest()
    {
        _mockLoginModel = new Mock<ILoginModel>();
        _mockConfig = new Mock<IConfiguration>();
        _controller = new LoginController(_mockConfig.Object, _mockLoginModel.Object);
    }

    [Fact]
    public async Task TestLogin_Success()
    {
        _mockLoginModel.Setup(m => m.LoginAsync("Manny2020", "20202020")).ReturnsAsync(true);

        var result = await _controller.Login(new LoginRequest("Manny2020", "20202020"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task TestLogin_Failure()
    {
        _mockLoginModel.Setup(m => m.LoginAsync("Ma", "20202020")).ReturnsAsync(false);

        var result = await _controller.Login(new LoginRequest("Ma", "20202020"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Signup_NewUser_Success()
    {
        _mockLoginModel.Setup(m => m.SignupAsync("newman", "pass1234")).ReturnsAsync(true);

        var result = await _controller.Signup(new LoginRequest("newman", "pass1234"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Signup_Conflict()
    {
        _mockLoginModel.Setup(m => m.SignupAsync("guest", "guestPassword")).ReturnsAsync(false);

        var result = await _controller.Signup(new LoginRequest("guest", "guestPassword"));

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Guest()
    {
        _mockLoginModel.Setup(m => m.SignupAsync("guest", "guestPassword")).ReturnsAsync(true);
        _mockLoginModel.Setup(m => m.LoginAsync("guest", "guestPassword")).ReturnsAsync(true);

        var result = await _controller.GuestLogin();

        Assert.IsType<OkObjectResult>(result);
    }
}
