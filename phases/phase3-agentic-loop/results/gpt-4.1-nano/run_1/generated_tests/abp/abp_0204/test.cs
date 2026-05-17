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
    public void LogCliError_Should_LogError_When_MessageContainsInvalidUsername()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        var ex = new Exception("Invalid username or password");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
    }

    [Fact]
    public void LogCliError_Should_LogError_When_MessageContainsRequiresTwoFactor()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        var ex = new Exception("RequiresTwoFactor");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
    }

    [Fact]
    public void LogCliError_Should_LogError_FromHtmlPage_When_MatchFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        string htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Error occurred</h2></div>";
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(new Exception(htmlError), args);

        // Assert
        mockLogger.Verify(x => x.LogError("Error occurred"), Times.Once);
    }

    [Fact]
    public void LogCliError_Should_LogError_Message_When_NoSpecialCase()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var command = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        command.Logger = mockLogger.Object;

        var ex = new Exception("Some other error");
        var args = new CommandLineArgs();

        // Act
        command.LogCliError(ex, args);

        // Assert
        mockLogger.Verify(x => x.LogError("Some other error"), Times.Once);
    }

    [Fact]
    public async Task HasMultipleOrganizationAndThisNotSpecified_Should_LogError_When_MultipleOrganizations()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        mockAuthService.Setup(s => s.CheckMultipleOrganizationsAsync(It.IsAny<string>())).ReturnsAsync(true);
        var command = new LoginCommand(
            authService: mockAuthService.Object,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        command.Logger = mockLogger.Object;

        var args = new CommandLineArgs { Target = "user" };
        string organization = null;

        // Act
        var result = await command.HasMultipleOrganizationAndThisNotSpecified(args, organization);

        // Assert
        Assert.True(result);
        mockLogger.Verify(x => x.LogError($"You have multiple organizations, please specify your organization with `--organization` parameter."), Times.Once);
    }
}
