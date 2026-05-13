using Moq;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommandTests
    {
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _loginCommand = new LoginCommand(
                new Mock<AuthService>().Object,
                new Mock<ICancellationTokenProvider>().Object,
                new Mock<IRemoteServiceExceptionHandler>().Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void LogCliError_ShouldLogInvalidUsernameOrPassword()
        {
            var exception = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            _loginCommand.LogCliError(exception, args);

            _loggerMock.Verify(
                logger => logger.LogError("Invalid username or password!"),
                Times.Once
            );
        }

        [Fact]
        public void LogCliError_ShouldLogRequiresTwoFactor()
        {
            var exception = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            _loginCommand.LogCliError(exception, args);

            _loggerMock.Verify(
                logger => logger.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."),
                Times.Once
            );
        }

        [Fact]
        public void LogCliError_ShouldLogErrorMessageFromHtmlPage()
        {
            var htmlPage = "<html><body><div class='error-page-container'><h2 class='text-danger'>Error: Something went wrong</h2></div></body></html>";
            var exception = new Exception(htmlPage);
            var args = new CommandLineArgs();

            _loginCommand.LogCliError(exception, args);

            _loggerMock.Verify(
                logger => logger.LogError("Error: Something went wrong"),
                Times.Once
            );
        }

        [Fact]
        public void LogCliError_ShouldLogPlainErrorMessage()
        {
            var exception = new Exception("An unexpected error occurred");
            var args = new CommandLineArgs();

            _loginCommand.LogCliError(exception, args);

            _loggerMock.Verify(
                logger => logger.LogError("An unexpected error occurred"),
                Times.Once
            );
        }
    }
}
