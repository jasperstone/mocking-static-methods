using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
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
        public async Task ExecuteAsync_LogsErrorWhenMultipleOrganizations()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.CheckMultipleOrganizationsAsync(commandLineArgs.Target))
                .ReturnsAsync(true);

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("You have multiple organizations, please specify your organization with `--organization` parameter."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenInvalidUsernameOrPassword()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("Invalid username or password"));

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenTwoFactorAuthenticationEnabled()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("RequiresTwoFactor"));

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenErrorMessageFromHtmlPage()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("<html><body><div class=\"error-page-container\"><h2 class=\"text-danger\">Error message</h2></div></body></html>"));

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Error message"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenDefault()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("Test exception"));

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Test exception"), Times.Once);
        }
    }
}
