using System;
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
        private readonly Mock<AuthService> _authServiceMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            // Provide dummy constructor arguments for AuthService
            var authServiceCtorArgs = new object[]
            {
                null, // IIdentityModelAuthenticationService
                null, // ILogger<AuthService>
                null, // ICancellationTokenProvider
                null, // CliHttpClientFactory
                null, // RemoteServiceExceptionHandler
                null  // IJsonSerializer
            };
            _authServiceMock = new Mock<AuthService>(authServiceCtorArgs);

            _loginCommand = new LoginCommand(
                _authServiceMock.Object,
                cancellationTokenProvider: null,
                remoteServiceExceptionHandler: null
            )
            {
                Logger = _loggerMock.Object
            };
        }

        private void VerifyLogError(string expectedMessage, Exception ex)
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private void VerifyLogError(string expectedMessage)
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCliError_LogsErrorMessage_WhenExceptionMessageIsGeneric()
        {
            // Arrange
            var ex = new Exception("Some generic error message");
            var args = new CommandLineArgs();

            // Act
            var logCliErrorMethod = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logCliErrorMethod.Invoke(_loginCommand, new object[] { ex, args });

            // Assert
            VerifyLogError("Some generic error message", ex);
        }

        [Fact]
        public void LogCliError_LogsInvalidUsernameOrPassword_WhenExceptionMessageContainsInvalidUsernameOrPassword()
        {
            // Arrange
            var ex = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            // Act
            var logCliErrorMethod = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logCliErrorMethod.Invoke(_loginCommand, new object[] { ex, args });

            // Assert
            VerifyLogError("Invalid username or password!");
        }

        [Fact]
        public void LogCliError_LogsTwoFactorMessage_WhenExceptionMessageContainsRequiresTwoFactor()
        {
            // Arrange
            var ex = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            // Act
            var logCliErrorMethod = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logCliErrorMethod.Invoke(_loginCommand, new object[] { ex, args });

            // Assert
            VerifyLogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login.");
        }

        [Fact]
        public void LogCliError_LogsErrorMessageFromHtml_WhenExceptionMessageContainsErrorPageContainer()
        {
            // Arrange
            var htmlMessage = "<html><div class=\"error-page-container\"><h2 class=\"text-danger\">Test error message</h2></div></html>";
            var ex = new Exception(htmlMessage);
            var args = new CommandLineArgs();

            // Act
            var logCliErrorMethod = typeof(LoginCommand).GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            logCliErrorMethod.Invoke(_loginCommand, new object[] { ex, args });

            // Assert
            VerifyLogError("Test error message");
        }
    }
}
