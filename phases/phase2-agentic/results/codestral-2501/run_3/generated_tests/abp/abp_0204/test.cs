using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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
        public async Task HasMultipleOrganizationAndThisNotSpecified_ShouldLogError_WhenMultipleOrganizations()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs(new[] { "login", "username" });
            _authServiceMock.Setup(x => x.CheckMultipleOrganizationsAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, null);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenInvalidUsernameOrPassword()
        {
            // Arrange
            var ex = new Exception("Invalid username or password");
            var commandLineArgs = new CommandLineArgs(new[] { "login", "username" });

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenRequiresTwoFactor()
        {
            // Arrange
            var ex = new Exception("RequiresTwoFactor");
            var commandLineArgs = new CommandLineArgs(new[] { "login", "username" });

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenHtmlPageContainsError()
        {
            // Arrange
            var ex = new Exception("<html><body><div class=\"error-page-container\"><h2 class=\"text-danger\">Error Message</h2></div></body></html>");
            var commandLineArgs = new CommandLineArgs(new[] { "login", "username" });

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenGeneralException()
        {
            // Arrange
            var ex = new Exception("General error message");
            var commandLineArgs = new CommandLineArgs(new[] { "login", "username" });

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
