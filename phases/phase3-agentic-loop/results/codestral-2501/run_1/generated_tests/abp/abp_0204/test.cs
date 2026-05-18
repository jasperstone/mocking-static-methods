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
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _authServiceMock = new Mock<AuthService>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();

            _loginCommand = new LoginCommand(
                _authServiceMock.Object,
                _cancellationTokenProviderMock.Object,
                null
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenMultipleOrganizationsAndNoOrganizationSpecified()
        {
            // Arrange
            var commandLineArgs = new Mock<CommandLineArgs>();
            commandLineArgs.Setup(x => x.Target).Returns("testUser");
            commandLineArgs.Setup(x => x.Options).Returns(new System.Collections.Generic.Dictionary<string, string>());

            _authServiceMock.Setup(x => x.CheckMultipleOrganizationsAsync(commandLineArgs.Object.Target))
                .ReturnsAsync(true);

            // Act
            await _loginCommand.ExecuteAsync(commandLineArgs.Object);

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
            var args = new Mock<CommandLineArgs>().Object;

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
            var args = new Mock<CommandLineArgs>().Object;

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
            var ex = new Exception("<html><body><h2 class=\"text-danger\">Error Message</h2></body></html>");
            var args = new Mock<CommandLineArgs>().Object;

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Message")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogCliError_ShouldLogError_WhenExceptionMessageDoesNotMatchAnyCondition()
        {
            // Arrange
            var ex = new Exception("Some other error message");
            var args = new Mock<CommandLineArgs>().Object;

            // Act
            _loginCommand.LogCliError(ex, args);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Some other error message")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
