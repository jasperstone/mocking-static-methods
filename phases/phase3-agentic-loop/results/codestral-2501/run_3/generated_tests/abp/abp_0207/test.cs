using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLoggedIn_LogsLoginInfo()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var commandLineArgs = new CommandLineArgs();

        authServiceMock.Setup(x => AuthService.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "Example Org"
        });

        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Login info:") &&
                                   s.Contains("Name: John") &&
                                   s.Contains("Surname: Doe") &&
                                   s.Contains("Username: johndoe") &&
                                   s.Contains("Email Address: john.doe@example.com") &&
                                   s.Contains("Organization: Example Org")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var commandLineArgs = new CommandLineArgs();

        authServiceMock.Setup(x => AuthService.IsLoggedIn()).Returns(false);

        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("You are not logged in.")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoginInfoIsNull_LogsError()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var commandLineArgs = new CommandLineArgs();

        authServiceMock.Setup(x => AuthService.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("Unable to get login info.")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
