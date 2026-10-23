using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Text.RegularExpressions;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginCommandTests
{
    private readonly Mock<ILogger<LoginCommand>> _mockLogger;

    public LoginCommandTests()
    {
        _mockLogger = new Mock<ILogger<LoginCommand>>();
    }

    [Fact]
    public void LogCliError_LogsExceptionMessage_WhenNoSpecialCasesMatch()
    {
        // Arrange
        var loginCommand = CreateLoginCommand();
        var ex = new Exception("Some generic error message");

        // Act
        loginCommand.LogCliError(ex, new CommandLineArgs(Array.Empty<string>()));

        // Assert - Tests the specific LogError(ex.Message) call (fallback case on line 137)
        _mockLogger.Verify(l => l.LogError("Some generic error message"), Times.Once);
    }

    [Fact]
    public void LogCliError_LogsInvalidCredentialsMessage_WhenExceptionContainsInvalidUsernameOrPassword()
    {
        // Arrange
        var loginCommand = CreateLoginCommand();
        var ex = new Exception("Invalid username or password");

        // Act
        loginCommand.LogCliError(ex, new CommandLineArgs(Array.Empty<string>()));

        // Assert
        _mockLogger.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
    }

    [Fact]
    public void LogCliError_LogsTwoFactorMessage_WhenExceptionContainsRequiresTwoFactor()
    {
        // Arrange
        var loginCommand = CreateLoginCommand();
        var ex = new Exception("RequiresTwoFactor");

        // Act
        loginCommand.LogCliError(ex, new CommandLineArgs(Array.Empty<string>()));

        // Assert
        _mockLogger.Verify(l => l.LogError(It.Is<string>(m => m.Contains("Two factor authentication is enabled"))), Times.Once);
    }

    [Fact]
    public void LogCliError_LogsHtmlErrorMessage_WhenHtmlPageContainsError()
    {
        // Arrange
        var loginCommand = CreateLoginCommand();
        var htmlErrorPage = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Test Error</h2></div>";
        var ex = new Exception(htmlErrorPage);

        // Act
        loginCommand.LogCliError(ex, new CommandLineArgs(Array.Empty<string>()));

        // Assert
        _mockLogger.Verify(l => l.LogError("Test Error"), Times.Once);
    }
}

// Test-specific LoginCommand that exposes private methods and uses testable dependencies
public class TestableLoginCommand : LoginCommand
{
    public new ILogger<LoginCommand> Logger { get; set; }

    public TestableLoginCommand(Mock<ILogger<LoginCommand>> mockLogger) : base(
        new DummyAuthService(),
        new DummyCancellationTokenProvider(),
        new DummyRemoteServiceExceptionHandler())
    {
        Logger = mockLogger.Object;
    }

    public new void LogCliError(Exception ex, CommandLineArgs args)
    {
        base.LogCliError(ex, args);
    }
}

public class LoginCommandTestsHelper
{
    public static TestableLoginCommand CreateLoginCommand()
    {
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        return new TestableLoginCommand(mockLogger);
    }
}

// Dummy implementations for constructor dependencies
public class DummyAuthService
{
    public Task<bool> CheckMultipleOrganizationsAsync(string target) => Task.FromResult(false);
    public Task LoginAsync(string target, string password, string organization) => Task.CompletedTask;
    public Task DeviceLoginAsync() => Task.CompletedTask;
    public Task<(string Username)> GetLoginInfoAsync() => Task.FromResult((Username: "test"));
}

public class DummyCancellationTokenProvider : ICancellationTokenProvider
{
    public CancellationToken Token => default;
}

public class DummyRemoteServiceExceptionHandler : IRemoteServiceExceptionHandler
{
}
