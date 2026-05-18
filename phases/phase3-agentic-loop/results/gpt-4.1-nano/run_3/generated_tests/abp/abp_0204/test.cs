using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

public class LoginCommandTests
{
    [Fact]
    public async Task LogCliError_Should_LogError_When_MessageContainsInvalidUsernameOrPassword()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        var ex = new Exception("Invalid username or password");
        var args = CommandLineArgs.Empty();

        // Act
        command.GetType().GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(command, new object[] { ex, args });

        // Assert
        mockLogger.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
    }

    [Fact]
    public async Task LogCliError_Should_LogError_When_MessageContainsRequiresTwoFactor()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        var ex = new Exception("RequiresTwoFactor");
        var args = CommandLineArgs.Empty();

        // Act
        command.GetType().GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(command, new object[] { ex, args });

        // Assert
        mockLogger.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
    }

    [Fact]
    public async Task LogCliError_Should_LogError_FromHtmlPage_When_MessageContainsErrorPage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        string htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Error occurred</h2></div>";
        var ex = new Exception(htmlError);
        var args = CommandLineArgs.Empty();

        // Act
        command.GetType().GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(command, new object[] { ex, args });

        // Assert
        mockLogger.Verify(x => x.LogError("Error occurred"), Times.Once);
    }

    [Fact]
    public async Task LogCliError_Should_LogError_Message_When_NoSpecificConditionMet()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        string errorMsg = "Some other error";
        var ex = new Exception(errorMsg);
        var args = CommandLineArgs.Empty();

        // Act
        command.GetType().GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(command, new object[] { ex, args });

        // Assert
        mockLogger.Verify(x => x.LogError(errorMsg), Times.Once);
    }
}
