using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Microsoft.Extensions.Logging;
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

        mockAuthService.Setup(authService => authService.IsLoggedIn()).Returns(true);
        mockAuthService.Setup(authService => authService.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "Example Org"
        });

        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(logger => logger.LogInformation(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

        mockAuthService.Setup(authService => authService.IsLoggedIn()).Returns(false);

        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(logger => logger.LogError("You are not logged in."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoginInfoIsNull_LogsError()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

        mockAuthService.Setup(authService => authService.IsLoggedIn()).Returns(true);
        mockAuthService.Setup(authService => authService.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(logger => logger.LogError("Unable to get login info."), Times.Once);
    }
}
