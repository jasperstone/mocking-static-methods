using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Threading;
using System;

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
            _authServiceMock = new Mock<AuthService>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            _loginCommand = new LoginCommand(
                _authServiceMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenMultipleOrganizationsAndNoOrganizationSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "testUser",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };

            _authServiceMock.Setup(x => x.CheckMultipleOrganizationsAsync(commandLineArgs.Target))
                .ReturnsAsync(true);

            // Act
            await _loginCommand.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You have multiple organizations, please specify your organization with `--organization` parameter.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenExceptionMessageContainsInvalidUsernameOrPassword()
        {
            // Arrange
            var ex = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid username or password!")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenExceptionMessageContainsRequiresTwoFactor()
        {
            // Arrange
            var ex = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Two factor authentication is enabled for your account. Please use `abp login --device` command to login.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenExceptionMessageContainsHtmlPage()
        {
            // Arrange
            var ex = new Exception("<h2 class=\"text-danger\">Test Error</h2>");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test Error")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenExceptionMessageDoesNotMatchAnyCondition()
        {
            // Arrange
            var ex = new Exception("Some other error");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Some other error")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
