using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_NotLoggedIn_LogsError()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object);
        command.Logger = loggerMock.Object;

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LoginInfoIsNull_LogsError()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object);
        command.Logger = loggerMock.Object;

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(l => l.LogError("Unable to get login info."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LoginInfoIsNotNull_LogsInfo()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo
        {
            Name = "Name",
            Surname = "Surname",
            Username = "Username",
            EmailAddress = "EmailAddress",
            Organization = "Organization"
        });
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object);
        command.Logger = loggerMock.Object;

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
