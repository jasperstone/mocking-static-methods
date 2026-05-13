using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Threading;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommandTests
    {
        private readonly Mock<AuthService> _authServiceMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<IRemoteServiceExceptionHandler> _remoteServiceExceptionHandlerMock;
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;

        public LoginCommandTests()
        {
#pragma warning disable CS8625
            _authServiceMock = new Mock<AuthService>(MockBehavior.Strict, null, null, null, null, null, null, null);
#pragma warning restore CS8625
            _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();
            _loggerMock = new Mock<ILogger<LoginCommand>>();
        }

        [Fact]
        public async Task ExecuteAsync_Should_Log_Error_For_General_Exception_Message()
        {
            // Arrange
            const string username = "john";
            const string password = "1234";
            const string generalErrorMessage = "Some unexpected error.";
            _authServiceMock
                .Setup(s => s.LoginAsync(username, password, null))
                .ThrowsAsync(new Exception(generalErrorMessage));

            var commandLineArgs = new CommandLineArgs(username);
            commandLineArgs.Options.Add("p", password);

            var command = new LoginCommand(
                _authServiceMock.Object,
                _cancellationTokenProviderMock.Object,
                _remoteServiceExceptionHandlerMock.Object)
            {
                Logger = _loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(commandLineArgs);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString() == generalErrorMessage),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _authServiceMock.VerifyAll();
        }
    }
}
