using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;
using Shimmy;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_NotLoggedIn_LogsError()
    {
        // Arrange
        using var shim = Shim.Replace(() => AuthService.IsLoggedIn()).With(() => false);
        var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(
            x => x.LogError("You are not logged in."),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LoggedInButNoLoginInfo_LogsError()
    {
        // Arrange
        using var shim = Shim.Replace(() => AuthService.IsLoggedIn()).With(() => true);
        var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
        authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(
            x => x.LogError("Unable to get login info."),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LoggedInAndHasLoginInfo_LogsLoginInfo()
    {
        // Arrange
        using var shim = Shim.Replace(() => AuthService.IsLoggedIn()).With(() => true);
        var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "Example Org"
        };
        authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(It.Is<string>(s => s.Contains("Login info:"))),
            Times.Once);
    }
}
