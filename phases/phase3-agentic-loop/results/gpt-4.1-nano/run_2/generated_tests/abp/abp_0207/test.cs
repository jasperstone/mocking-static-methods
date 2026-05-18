using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;

public class LoginInfoCommandTests
{
    private readonly Mock<ILogger<LoginInfoCommand>> _loggerMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly LoginInfoCommand _command;

    public LoginInfoCommandTests()
    {
        _loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        _authServiceMock = new Mock<IAuthService>();
        _command = new LoginInfoCommand(_authServiceMock.Object)
        {
            Logger = _loggerMock.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_When_LoggedInAndLoginInfoIsNotNull()
    {
        // Arrange
        _authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
        var loginInfo = new
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john@example.com",
            Organization = "ExampleOrg"
        };
        _authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("Login info:") && s.Contains("Name: John"))),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        _authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _loggerMock.Verify(
            x => x.LogError("You are not logged in."),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_When_LoginInfoIsNull()
    {
        // Arrange
        _authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
        _authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _loggerMock.Verify(
            x => x.LogError("Unable to get login info."),
            Times.Once);
    }
}
