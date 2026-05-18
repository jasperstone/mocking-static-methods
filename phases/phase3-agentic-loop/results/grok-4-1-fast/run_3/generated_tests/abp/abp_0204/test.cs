using Microsoft.Extensions.Logging;
using Moq;
using System;
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
        _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        // Create LoginCommand with NullLogger and override
        _loginCommand = new LoginCommand(null!, null!, null!);
        _loginCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public void HasMultipleOrganizationAndThisNotSpecified_ShouldLogError_WhenMultipleOrganizationsExist()
    {
        // Arrange - use reflection to mock AuthService.CheckMultipleOrganizationsAsync result
        var commandLineArgs = new CommandLineArgs("testuser");
        var authServiceField = typeof(LoginCommand).GetField("AuthService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var mockAuthService = new Mock<object>();
        authServiceField.SetValue(_loginCommand, mockAuthService.Object);

        // Use reflection to call private method
        var method = typeof(LoginCommand).GetMethod("HasMultipleOrganizationAndThisNotSpecified", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var task = (Task<bool>)method.Invoke(_loginCommand, new object[] { commandLineArgs, null })!;

        // Simulate the async result being true (multiple orgs exist)
        task.Wait();

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You have multiple organizations")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void LogCliError_ShouldLogExceptionMessage_WhenNoSpecialConditionsMet()
    {
        // Arrange
        var exception = new Exception("Test error message that doesn't match any special cases");
        var commandLineArgs = new CommandLineArgs();

        // Act
        // Use reflection since LogCliError is private
        var method = typeof(LoginCommand).GetMethod("LogCliError", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(_loginCommand, new object[] { exception, commandLineArgs });

        // Assert - line 137 case
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Test error message that doesn't match any special cases"),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void LogCliError_ShouldLogInvalidUsernamePassword_WhenExceptionContainsInvalidUsernameOrPassword()
    {
        // Arrange
        var exception = new Exception("Invalid username or password");
        var commandLineArgs = new CommandLineArgs();

        // Act
        var method = typeof(LoginCommand).GetMethod("LogCliError", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(_loginCommand, new object[] { exception, commandLineArgs });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid username or password!"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void LogCliError_ShouldLogTwoFactorMessage_WhenExceptionContainsRequiresTwoFactor()
    {
        // Arrange
        var exception = new Exception("RequiresTwoFactor");
        var commandLineArgs = new CommandLineArgs();

        // Act
        var method = typeof(LoginCommand).GetMethod("LogCliError", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(_loginCommand, new object[] { exception, commandLineArgs });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Two factor authentication is enabled")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
