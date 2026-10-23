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
        _loggerMock = new Mock<ILogger<LoginCommand>>();
        var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
        var cancellationTokenProviderMock = new Mock<object>();
        var remoteServiceExceptionHandlerMock = new Mock<object>();

        _loginCommand = new LoginCommand(
            authServiceMock.Object,
            (ICancellationTokenProvider)cancellationTokenProviderMock.Object,
            (IRemoteServiceExceptionHandler)remoteServiceExceptionHandlerMock.Object
        );
        _loginCommand.Logger = _loggerMock.Object;
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
