using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommandTests
    {
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _authServiceMock = new Mock<AuthService>(MockBehavior.Strict, null, null, null);
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            _loginCommand = new LoginCommand(
                _authServiceMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void LogCliError_LogsInvalidUsernameOrPasswordMessage()
        {
            var ex = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            _loggerMock.Setup(l => l.LogError("Invalid username or password!"));

            _loginCommand.GetType()
                .GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_loginCommand, new object[] { ex, args });

            _loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public void LogCliError_LogsTwoFactorAuthenticationMessage()
        {
            var ex = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            _loggerMock.Setup(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."));

            _loginCommand.GetType()
                .GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_loginCommand, new object[] { ex, args });

            _loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public void LogCliError_LogsErrorMessageFromHtmlPage()
        {
            var htmlMessage = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Some error<eof/></h2></div>";
            var ex = new Exception(htmlMessage);
            var args = new CommandLineArgs();

            _loggerMock.Setup(l => l.LogError("Some error"));

            _loginCommand.GetType()
                .GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_loginCommand, new object[] { ex, args });

            _loggerMock.Verify(l => l.LogError("Some error"), Times.Once);
        }

        [Fact]
        public void LogCliError_LogsExceptionMessageWhenNoOtherConditionMatches()
        {
            var ex = new Exception("Some other error message");
            var args = new CommandLineArgs();

            _loggerMock.Setup(l => l.LogError("Some other error message"));

            _loginCommand.GetType()
                .GetMethod("LogCliError", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_loginCommand, new object[] { ex, args });

            _loggerMock.Verify(l => l.LogError("Some other error message"), Times.Once);
        }
    }
}
