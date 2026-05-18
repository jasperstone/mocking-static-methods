using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommand_LogCliError_Tests
    {
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommand_LogCliError_Tests()
        {
            var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
            var cancellationTokenProviderMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>(); // fallback to a known interface to avoid missing type
            var remoteServiceExceptionHandlerMock = new Mock<object>(); // fallback to object to avoid missing type

            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _loginCommand = new LoginCommand(authServiceMock.Object, null, null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Theory]
        [InlineData("Invalid username or password", "Invalid username or password!")]
        [InlineData("RequiresTwoFactor", "Two factor authentication is enabled for your account. Please use `abp login --device` command to login.")]
        [InlineData("<div class=\"error-page-container\"><h2 class=\"text-danger\">Some error<eof/></h2></div>", "Some error")]
        [InlineData("Some other error message", "Some other error message")]
        public void LogCliError_LogsExpectedErrorMessage(string exceptionMessage, string expectedLogMessage)
        {
            // Arrange
            var ex = new Exception(exceptionMessage);
            var args = new CommandLineArgs();

            // Act
            var logCliErrorMethod = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logCliErrorMethod.Invoke(_loginCommand, new object[] { ex, args });

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
}
