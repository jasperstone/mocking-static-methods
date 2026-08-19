using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class LoginCommandTests
{
    [Fact]
    public void LogCliError_LogsErrorMessage_WhenExceptionMessageContainsInvalidUsernameOrPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, null)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid username or password")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_LogsErrorMessage_WhenExceptionMessageContainsRequiresTwoFactor()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, null)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Two factor authentication is enabled for your account. Please use `abp login --device` command to login.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_LogsErrorMessage_WhenExceptionMessageContainsHtmlPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, null)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("<h2 class=\"text-danger\">Some error message</h2>");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Some error message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_LogsErrorMessage_WhenExceptionMessageDoesNotMatchAnyCondition()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, null)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("Some other error message");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Some other error message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
