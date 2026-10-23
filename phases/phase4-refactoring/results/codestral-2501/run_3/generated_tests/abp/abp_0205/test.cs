using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using System.IO;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_NotLoggedIn_LogsError()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(x => x.IsLoggedIn()).Returns(false);
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
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
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
    public async Task ExecuteAsync_LoggedInAndHasLoginInfo_LogsInformation()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
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
            x => x.LogInformation(It.IsAny<string>()),
            Times.Once);
    }
}
