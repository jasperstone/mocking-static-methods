using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandTests
    {
        [Fact]
        public async Task HasMultipleOrganizationAndThisNotSpecified_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var commandLineArgs = new CommandLineArgs
            {
                Target = "test",
                Options = new AbpCommandLineOptions()
            };

            authServiceMock
                .Setup(a => a.CheckMultipleOrganizationsAsync(commandLineArgs.Target))
                .ReturnsAsync(true);

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("You have multiple organizations, please specify your organization with `--organization` parameter."), Times.Once);
        }

        [Fact]
        public void LogCliError_InvalidUsernameOrPassword_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var exception = new Exception("Invalid username or password");
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public void LogCliError_TwoFactorAuthenticationEnabled_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var exception = new Exception("RequiresTwoFactor");
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public void LogCliError_HtmlPageError_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var exception = new Exception("<html><body><h2 class=\"text-danger\">Error message</h2></body></html>");
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Error message"), Times.Once);
        }

        [Fact]
        public void LogCliError_GenericError_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var exception = new Exception("Generic error message");
            var commandLineArgs = new CommandLineArgs();

            var loginCommand = new LoginCommand(null, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Generic error message"), Times.Once);
        }
    }
}
