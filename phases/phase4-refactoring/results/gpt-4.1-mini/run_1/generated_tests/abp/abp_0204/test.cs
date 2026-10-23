using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommandTests
    {
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();

            // Pass null for dependencies not used in LogCliError
            _loginCommand = new LoginCommand(
                authService: null,
                cancellationTokenProvider: null,
                remoteServiceExceptionHandler: null
            );
            _loginCommand.Logger = _loggerMock.Object;
        }

        private void VerifyLogError(string expectedMessage)
        {
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCliError_LogsInvalidUsernameOrPasswordError()
        {
            var ex = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_loginCommand, new object[] { ex, args });

            VerifyLogError("Invalid username or password!");
        }

        [Fact]
        public void LogCliError_LogsTwoFactorAuthenticationError()
        {
            var ex = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_loginCommand, new object[] { ex, args });

            VerifyLogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login.");
        }

        [Fact]
        public void LogCliError_LogsErrorMessageFromHtmlPage()
        {
            var htmlErrorMessage = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Test error message<eof/></h2></div>";
            var ex = new Exception(htmlErrorMessage);
            var args = new CommandLineArgs();

            var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_loginCommand, new object[] { ex, args });

            VerifyLogError("Test error message");
        }

        [Fact]
        public void LogCliError_LogsExceptionMessageWhenNoOtherConditionMatches()
        {
            var ex = new Exception("Some other error");
            var args = new CommandLineArgs();

            var method = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_loginCommand, new object[] { ex, args });

            VerifyLogError("Some other error");
        }
    }
}
