using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLoggedIn_LogsLoginInfo()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "Example Org"
        };

        mockAuthService.Setup(x => AuthService.IsLoggedIn()).Returns(true);
        mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Login info:") &&
                                    s.Contains("Name: John") &&
                                    s.Contains("Surname: Doe") &&
                                    s.Contains("Username: johndoe") &&
                                    s.Contains("Email Address: john.doe@example.com") &&
                                    s.Contains("Organization: Example Org")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        mockAuthService.Setup(x => AuthService.IsLoggedIn()).Returns(false);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("You are not logged in.")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoginInfoIsNull_LogsError()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        mockAuthService.Setup(x => AuthService.IsLoggedIn()).Returns(true);
        mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("Unable to get login info.")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
