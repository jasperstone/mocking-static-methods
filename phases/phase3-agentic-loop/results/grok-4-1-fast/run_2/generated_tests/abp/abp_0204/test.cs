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
        _loginCommand = new LoginCommand(null!, null!, null!);
        _loginCommand.Logger = _loggerMock.Object;
    }

    [Fact]
    public async Task HasMultipleOrganizationAndThisNotSpecified_Should_LogError_When_MultipleOrganizationsExist()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs { Target = "testuser" };
        var organization = "";

        // Act
        var result = await _loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, organization);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You have multiple organizations, please specify your organization with `--organization` parameter.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void LogCliError_Should_LogError_For_InvalidUsernameOrPassword()
    {
        // Arrange
        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs();

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid username or password!"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_Should_LogError_For_TwoFactorRequired()
    {
        // Arrange
        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs();

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Two factor authentication is enabled for your account. Please use `abp login --device` command to login.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_Should_LogHtmlErrorMessage_When_HtmlPageDetected()
    {
        // Arrange
        var htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Test Error</h2></div>";
        var ex = new Exception(htmlError);
        var args = new CommandLineArgs();

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Test Error"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogCliError_Should_LogExceptionMessage_When_NoSpecialCases()
    {
        // Arrange
        var ex = new Exception("Generic error message");
        var args = new CommandLineArgs();

        // Act
        _loginCommand.LogCliError(ex, args);

        // Assert - Tests line 137 specifically (Logger.LogError(ex.Message))
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Generic error message"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
