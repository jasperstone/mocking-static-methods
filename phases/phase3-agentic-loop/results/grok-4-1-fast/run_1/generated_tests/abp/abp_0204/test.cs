using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginCommandTests
{
    private readonly Mock<ILogger<LoginCommand>> _loggerMock;

    public LoginCommandTests()
    {
        _loggerMock = new Mock<ILogger<LoginCommand>>();
    }

    [Fact]
    public void LogCliError_FallbackCase_ShouldLogExceptionMessage()
    {
        // Arrange
        var loginCommand = new LoginCommand(null!, null!, null!);
        loginCommand.Logger = _loggerMock.Object;
        
        var exception = new Exception("Generic test error");
        var args = new CommandLineArgs("testuser");

        // Act
        loginCommand.LogCliError(exception, args);

        // Assert - Verifies the LogError call on line 137 (fallback case)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString() == "Generic test error"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_InvalidCredentialsCase_ShouldLogSpecificMessage()
    {
        // Arrange
        var loginCommand = new LoginCommand(null!, null!, null!);
        loginCommand.Logger = _loggerMock.Object;
        
        var exception = new Exception("Invalid username or password");
        var args = new CommandLineArgs("testuser");

        // Act
        loginCommand.LogCliError(exception, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString() == "Invalid username or password!"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_TwoFactorCase_ShouldLogTwoFactorMessage()
    {
        // Arrange
        var loginCommand = new LoginCommand(null!, null!, null!);
        loginCommand.Logger = _loggerMock.Object;
        
        var exception = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs("testuser");

        // Act
        loginCommand.LogCliError(exception, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString().Contains("Two factor authentication is enabled for your account")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
