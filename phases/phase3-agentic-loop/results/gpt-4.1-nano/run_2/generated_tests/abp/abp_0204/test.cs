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
    public async Task LogError_Should_Call_LogError_With_Correct_Message_When_Exception_Message_Contains_InvalidUsername()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var loginCommand = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        loginCommand.Logger = mockLogger.Object;

        var exception = new Exception("Invalid username or password");
        var commandLineArgs = new CommandLineArgs();

        // Act
        loginCommand.LogCliError(exception, commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Invalid username or password!"),
            Times.Once);
    }

    [Fact]
    public async Task LogError_Should_Call_LogError_With_Correct_Message_When_Exception_Message_Contains_RequiresTwoFactor()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var loginCommand = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        loginCommand.Logger = mockLogger.Object;

        var exception = new Exception("RequiresTwoFactor");
        var commandLineArgs = new CommandLineArgs();

        // Act
        loginCommand.LogCliError(exception, commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."),
            Times.Once);
    }

    [Fact]
    public async Task LogError_Should_Call_LogError_With_ErrorMessage_From_HtmlPage_When_TryGetErrorMessageFromHtmlPage_Returns_True()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var loginCommand = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        loginCommand.Logger = mockLogger.Object;

        var exception = new Exception("<html><body><div class='error-page-container'><h2 class='text-danger'>Error occurred</h2></div></body></html>");
        var commandLineArgs = new CommandLineArgs();

        // Act
        loginCommand.LogCliError(exception, commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Error occurred"),
            Times.Once);
    }

    [Fact]
    public async Task LogError_Should_Call_LogError_With_Exception_Message_When_No_Specific_Match()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginCommand>>();
        var loginCommand = new LoginCommand(
            authService: null,
            cancellationTokenProvider: null,
            remoteServiceExceptionHandler: null);
        loginCommand.Logger = mockLogger.Object;

        var exception = new Exception("Some other error");
        var commandLineArgs = new CommandLineArgs();

        // Act
        loginCommand.LogCliError(exception, commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogError("Some other error"),
            Times.Once);
    }
}
