using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandTests
    {
        [Fact]
        public async Task Test_LogCliError_InvalidUsernameOrPassword()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            var exception = new Exception("Invalid username or password");

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task Test_LogCliError_TwoFactorAuthenticationEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            var exception = new Exception("RequiresTwoFactor");

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task Test_LogCliError_ErrorMessageFromHtmlPage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            var exception = new Exception("<html><body><h2 class=\"text-danger\">Error message</h2></body></html>");

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Error message"), Times.Once);
        }

        [Fact]
        public async Task Test_LogCliError_DefaultErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            var exception = new Exception("Test exception");

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Test exception"), Times.Once);
        }
    }
}
