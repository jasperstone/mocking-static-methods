using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Xunit;
using System.Web;
using System.Text.RegularExpressions;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginCommandTests
{
    private readonly Mock<ILogger<LoginCommand>> _loggerMock;
    private readonly LoginCommand _loginCommand;

    public LoginCommandTests()
    {
        _loggerMock = new Mock<ILogger<LoginCommand>>();
        _loginCommand = new LoginCommand(
            new AuthService(null!, NullLogger<AuthService>.Instance, null!, null!, null!, null!),
            null!,
            null!
        );
        _loginCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public void LogCliError_ShouldLogError_ForInvalidUsernamePassword()
    {
        // Arrange
        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs(new[] { "testuser" });

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "Invalid username or password!"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_ShouldLogError_ForTwoFactorAuth()
    {
        // Arrange
        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs(new[] { "testuser" });

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Two factor authentication is enabled for your account. Please use `abp login --device` command to login.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_ShouldLogHtmlErrorMessage_WhenHtmlPageContainsError()
    {
        // Arrange
        var htmlError = @"<div class=""error-page-container""><h2 class=""text-danger"">Test Error Message</h2></div>";
        var ex = new Exception(htmlError);
        var args = new CommandLineArgs(new[] { "testuser" });

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "Test Error Message"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_ShouldLogExceptionMessage_WhenNoSpecialCasesMatch()
    {
        // Arrange - Tests line 137 coverage: Logger.LogError(ex.Message);
        var ex = new Exception("Generic error that doesn't match special cases");
        var args = new CommandLineArgs(new[] { "testuser" });

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString() == "Generic error that doesn't match special cases"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryGetErrorMessageFromHtmlPage_ShouldReturnFalse_WhenNoErrorContainer()
    {
        // Arrange
        var htmlPage = "<div>no error</div>";

        // Act
        var result = LoginCommand.TryGetErrorMessageFromHtmlPage(htmlPage, out var errorMessage);

        // Assert
        Assert.False(result);
        Assert.Null(errorMessage);
    }

    [Fact]
    public void TryGetErrorMessageFromHtmlPage_ShouldExtractError_WhenValidHtmlPresent()
    {
        // Arrange
        var htmlPage = @"<div class=""error-page-container""><h2 class=""text-danger"">Test Error <eof/></h2></div>";
        var expected = "Test Error";

        // Act
        var result = LoginCommand.TryGetErrorMessageFromHtmlPage(htmlPage, out var errorMessage);

        // Assert
        Assert.True(result);
        Assert.Equal(expected, errorMessage);
    }
}
