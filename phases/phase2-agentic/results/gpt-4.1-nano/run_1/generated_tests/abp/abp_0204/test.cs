using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandTests
    {
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _authServiceMock = new Mock<AuthService>();
            _loginCommand = new LoginCommand(_authServiceMock.Object, null, null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task LogError_CallsLogError_WhenExceptionContainsInvalidUsername()
        {
            // Arrange
            var ex = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task LogError_CallsLogError_WhenExceptionContainsRequiresTwoFactor()
        {
            // Arrange
            var ex = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task LogError_CallsLogError_WithHtmlErrorMessage_WhenHtmlPageContainsError()
        {
            // Arrange
            string htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Error occurred</h2></div>";
            var ex = new Exception(htmlError);
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(x => x.LogError("Error occurred"), Times.Once);
        }

        [Fact]
        public async Task LogError_CallsLogError_WithOriginalMessage_WhenNoSpecificError()
        {
            // Arrange
            var ex = new Exception("Some other error");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(x => x.LogError("Some other error"), Times.Once);
        }
    }
}
