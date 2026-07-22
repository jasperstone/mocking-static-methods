using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_InvalidUsernameOrPassword_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = new LoginCommand(authServiceMock.Object, Mock.Of<ICancellationTokenProvider>(), Mock.Of<IRemoteServiceExceptionHandler>());
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs("username", null);
            commandLineArgs.Options.Add(LoginCommand.Options.Password.Short, "password");

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, commandLineArgs.Options[LoginCommand.Options.Password.Short], null))
                .Throws(new Exception("Invalid username or password"));

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_TwoFactorAuthenticationEnabled_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = new LoginCommand(authServiceMock.Object, Mock.Of<ICancellationTokenProvider>(), Mock.Of<IRemoteServiceExceptionHandler>());
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs("username", null);
            commandLineArgs.Options.Add(LoginCommand.Options.Password.Short, "password");

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, commandLineArgs.Options[LoginCommand.Options.Password.Short], null))
                .Throws(new Exception("RequiresTwoFactor"));

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_HtmlPageError_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = new LoginCommand(authServiceMock.Object, Mock.Of<ICancellationTokenProvider>(), Mock.Of<IRemoteServiceExceptionHandler>());
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs("username", null);
            commandLineArgs.Options.Add(LoginCommand.Options.Password.Short, "password");

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, commandLineArgs.Options[LoginCommand.Options.Password.Short], null))
                .Throws(new Exception("<html><body><div class=\"error-page-container\"><h2>Test error</h2></div></body></html>"));

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Test error"), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_GenericError_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var command = new LoginCommand(authServiceMock.Object, Mock.Of<ICancellationTokenProvider>(), Mock.Of<IRemoteServiceExceptionHandler>());
            command.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs("username", null);
            commandLineArgs.Options.Add(LoginCommand.Options.Password.Short, "password");

            authServiceMock.Setup(a => a.LoginAsync(commandLineArgs.Target, commandLineArgs.Options[LoginCommand.Options.Password.Short], null))
                .Throws(new Exception("Test error"));

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(l => l.LogError("Test error"), Times.Once);
        }
    }
}
