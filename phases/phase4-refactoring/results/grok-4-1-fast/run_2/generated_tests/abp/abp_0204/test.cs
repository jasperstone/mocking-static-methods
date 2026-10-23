using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginCommandTests
{
    private readonly Mock<ILogger<LoginCommand>> _loggerMock;
    private readonly LoginCommand _loginCommand;

    public LoginCommandTests()
    {
        _loggerMock = new Mock<ILogger<LoginCommand>>();
        _loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _loginCommand = new LoginCommand(null!, null!, null!);
        _loginCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public void LogCliError_LogsErrorMessage_WhenNoneOfSpecialCasesMatch()
    {
        // Arrange
        var ex = new Exception("Some unexpected error");
        var args = new CommandLineArgs(new Dictionary<string, string> { { "target", "testuser" } });

        // Act
        // Use reflection to call private method
        var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(_loginCommand, [ex, args]);

        // Assert - Verifies the LogError call on line 137 (fallback case)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Some unexpected error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_LogsInvalidCredentialsMessage_WhenInvalidUsernameOrPassword()
    {
        // Arrange
        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs(new Dictionary<string, string> { { "target", "testuser" } });

        // Act
        var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(_loginCommand, [ex, args]);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v) == "Invalid username or password!"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_LogsTwoFactorMessage_WhenRequiresTwoFactor()
    {
        // Arrange
        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs(new Dictionary<string, string> { { "target", "testuser" } });

        // Act
        var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(_loginCommand, [ex, args]);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Two factor authentication is enabled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void HasMultipleOrganizationAndThisNotSpecified_LogsError_WhenMultipleOrganizationsExist()
    {
        // Arrange
        var args = new CommandLineArgs(new Dictionary<string, string> { { "target", "testuser" } });
        var organization = "";

        // Act
        var method = typeof(LoginCommand).GetMethod("HasMultipleOrganizationAndThisNotSpecified", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = (Task<bool>)method.Invoke(_loginCommand, [args, organization])!;
        task.Wait();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("multiple organizations")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
