using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class LoginCommand_LogCliError_Tests
{
    private readonly Mock<ILogger<LoginCommand>> _loggerMock;
    private readonly LoginCommand _loginCommand;

    public LoginCommand_LogCliError_Tests()
    {
        // Use null for dependencies since they are not used in LogCliError
        _loggerMock = new Mock<ILogger<LoginCommand>>();
        _loginCommand = new LoginCommand(null, null, null)
        {
            Logger = _loggerMock.Object
        };
    }

    [Theory]
    [InlineData("Invalid username or password", "Invalid username or password!")]
    [InlineData("RequiresTwoFactor", "Two factor authentication is enabled for your account. Please use `abp login --device` command to login.")]
    [InlineData("<div class=\"error-page-container\"><h2 class=\"text-danger\">Custom error message<eof/></h2></div>", "Custom error message")]
    [InlineData("Some other error message", "Some other error message")]
    public void LogCliError_LogsExpectedErrorMessage(string exceptionMessage, string expectedLogMessage)
    {
        // Arrange
        var ex = new Exception(exceptionMessage);
        var args = new CommandLineArgs();

        // Act
        _loginCommand.GetType()
            .GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_loginCommand, new object[] { ex, args });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedLogMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
