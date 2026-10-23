using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Threading;
using Xunit;

public class LoginCommandTests
{
    [Fact]
    public async Task LogCliError_LogsErrorMessage_WhenExceptionMessageContainsInvalidUsernameOrPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Invalid username or password!"),
            Times.Once);
    }

    [Fact]
    public async Task LogCliError_LogsErrorMessage_WhenExceptionMessageContainsRequiresTwoFactor()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."),
            Times.Once);
    }

    [Fact]
    public async Task LogCliError_LogsErrorMessage_WhenExceptionMessageContainsHtmlPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("<h2 class=\"text-danger\">Error Message</h2>");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Error Message"),
            Times.Once);
    }

    [Fact]
    public async Task LogCliError_LogsErrorMessage_WhenExceptionMessageDoesNotContainSpecialCases()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>();
        var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

        var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
        {
            Logger = mockLogger.Object
        };

        var ex = new Exception("Some other error message");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Some other error message"),
            Times.Once);
    }
}
