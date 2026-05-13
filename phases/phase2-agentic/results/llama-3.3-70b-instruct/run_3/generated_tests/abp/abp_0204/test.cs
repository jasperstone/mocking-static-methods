using Microsoft.Extensions.Logging;
using Moq;
using System;
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
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;

        public LoginCommandTests()
        {
            _authServiceMock = new Mock<AuthService>();
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
        }

        [Fact]
        public async Task HasMultipleOrganizationAndThisNotSpecified_LogsError()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            commandLineArgs.Target = "test";
            var loginCommand = new LoginCommand(_authServiceMock.Object, _cancellationTokenProviderMock.Object, _remoteServiceExceptionHandlerMock.Object);
            loginCommand.Logger = _loggerMock.Object;
            _authServiceMock.Setup(x => x.CheckMultipleOrganizationsAsync(commandLineArgs.Target)).ReturnsAsync(true);

            // Act
            await loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, null);

            // Assert
            _loggerMock.Verify(x => x.LogError("You have multiple organizations, please specify your organization with `--organization` parameter."), Times.Once);
        }

        [Fact]
        public void LogCliError_InvalidUsernameOrPassword_LogsError()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            var exception = new Exception("Invalid username or password");
            var loginCommand = new LoginCommand(_authServiceMock.Object, _cancellationTokenProviderMock.Object, _remoteServiceExceptionHandlerMock.Object);
            loginCommand.Logger = _loggerMock.Object;

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public void LogCliError_TwoFactorAuthenticationEnabled_LogsError()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            var exception = new Exception("RequiresTwoFactor");
            var loginCommand = new LoginCommand(_authServiceMock.Object, _cancellationTokenProviderMock.Object, _remoteServiceExceptionHandlerMock.Object);
            loginCommand.Logger = _loggerMock.Object;

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public void LogCliError_TryGetErrorMessageFromHtmlPage_LogsError()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            var exception = new Exception("<html><body><div class='error-page-container'><h2 class='text-danger'>Error message</h2></div></body></html>");
            var loginCommand = new LoginCommand(_authServiceMock.Object, _cancellationTokenProviderMock.Object, _remoteServiceExceptionHandlerMock.Object);
            loginCommand.Logger = _loggerMock.Object;

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError("Error message"), Times.Once);
        }

        [Fact]
        public void LogCliError_Default_LogsError()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs();
            var exception = new Exception("Test exception");
            var loginCommand = new LoginCommand(_authServiceMock.Object, _cancellationTokenProviderMock.Object, _remoteServiceExceptionHandlerMock.Object);
            loginCommand.Logger = _loggerMock.Object;

            // Act
            loginCommand.LogCliError(exception, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError("Test exception"), Times.Once);
        }
    }
}
