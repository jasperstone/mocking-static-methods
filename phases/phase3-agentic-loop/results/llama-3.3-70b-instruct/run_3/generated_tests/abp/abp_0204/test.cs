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
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            authServiceMock
                .Setup(a => a.CheckMultipleOrganizationsAsync(commandLineArgs.Target))
                .ReturnsAsync(true);

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("You have multiple organizations, please specify your organization with `--organization` parameter."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenInvalidUsernameOrPassword()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            authServiceMock
                .Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("Invalid username or password"));

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenTwoFactorAuthenticationEnabled()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            authServiceMock
                .Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("RequiresTwoFactor"));

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenTryGetErrorMessageFromHtmlPage()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            authServiceMock
                .Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("<html><body><h2 class=\"text-danger\">Error message</h2></body></html>"));

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Error message"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsExceptionMessage()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            authServiceMock
                .Setup(a => a.LoginAsync(commandLineArgs.Target, "password", null))
                .Throws(new Exception("Exception message"));

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null);
            loginCommand.Logger = loggerMock.Object;

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Exception message"), Times.Once);
        }
    }
}
