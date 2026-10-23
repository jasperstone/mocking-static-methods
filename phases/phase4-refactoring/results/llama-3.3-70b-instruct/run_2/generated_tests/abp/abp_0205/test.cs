using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsError_WhenLoginInfoIsNull()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        loggerMock.Verify(l => l.LogError("Unable to get login info."), Times.Once);
    }
}
