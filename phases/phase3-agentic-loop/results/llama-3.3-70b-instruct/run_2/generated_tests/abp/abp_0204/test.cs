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
        public async Task ExecuteAsync_InvalidUsernameOrPassword_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<IAuthService>();
            var commandLineArgs = new CommandLineArgs("login", "username");
            commandLineArgs.Options.Add(LoginCommand.Options.Password.Short, "password");

            authServiceMock
                .Setup(x => x.LoginAsync(commandLineArgs.Target, commandLineArgs.Options[LoginCommand.Options.Password.Short]))
                .Throws(new Exception("Invalid username or password"));

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_TwoFactorAuthenticationEnabled_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<IAuthService>();
            var commandLineArgs = new CommandLineArgs("login", "username");
            commandLineArgs.Options.Add(LoginCommand.Options.Password.Short, "password");

            authServiceMock
                .Setup(x => x.LoginAsync(commandLineArgs.Target, commandLineArgs.Options[LoginCommand.Options.Password.Short]))
                .Throws(new Exception("RequiresTwoFactor"));

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_HasMultipleOrganizationsAndNotSpecified_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<IAuthService>();
            var commandLineArgs = new CommandLineArgs("login", "username");

            authServiceMock
                .Setup(x => x.CheckMultipleOrganizationsAsync(commandLineArgs.Target))
                .ReturnsAsync(true);

            var loginCommand = new LoginCommand(authServiceMock.Object, null, null)
            {
                Logger = loggerMock.Object
            };

            // Act
            await loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(x => x.LogError("You have multiple organizations, please specify your organization with `--organization` parameter."), Times.Once);
        }
    }
}
