using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Threading;
using Xunit;

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
                _remoteServiceExceptionHandlerMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task HasMultipleOrganizationAndThisNotSpecified_ShouldLogError_WhenMultipleOrganizationsAndNoOrganizationSpecified()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "testUser" };
            _authServiceMock.Setup(x => x.CheckMultipleOrganizationsAsync(commandLineArgs.Target)).ReturnsAsync(true);

            // Act
            var result = await _loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, null);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task HasMultipleOrganizationAndThisNotSpecified_ShouldNotLogError_WhenSingleOrganization()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs { Target = "testUser" };
            _authServiceMock.Setup(x => x.CheckMultipleOrganizationsAsync(commandLineArgs.Target)).ReturnsAsync(false);

            // Act
            var result = await _loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, null);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
            Assert.False(result);
        }

        [Fact]
        public void LogCliError_ShouldLogInvalidUsernameOrPasswordError()
        {
            // Arrange
            var ex = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogTwoFactorAuthenticationError()
        {
            // Arrange
            var ex = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogHtmlErrorMessage()
        {
            // Arrange
            var ex = new Exception("<h2 class=\"text-danger\">Test Error</h2>");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogGeneralErrorMessage()
        {
            // Arrange
            var ex = new Exception("General error message");
            var args = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
