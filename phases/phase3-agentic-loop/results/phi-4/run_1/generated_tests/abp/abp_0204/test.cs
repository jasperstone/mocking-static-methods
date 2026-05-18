using Moq;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.ProjectBuilding;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginCommandTests
    {
        [Fact]
        public void LogCliError_ShouldLogInvalidUsernameOrPassword()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
            {
                Logger = loggerMock.Object
            };
            var exception = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            // Act
            command.LogCliError(exception, args);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Invalid username or password!"),
                Times.Once
            );
        }

        [Fact]
        public void LogCliError_ShouldLogTwoFactorAuthenticationRequired()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
            {
                Logger = loggerMock.Object
            };
            var exception = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            // Act
            command.LogCliError(exception, args);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."),
                Times.Once
            );
        }

        [Fact]
        public void LogCliError_ShouldLogErrorMessageFromHtmlPage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
            {
                Logger = loggerMock.Object
            };
            var htmlPage = "<html><body><div class='error-page-container'><h2 class='text-danger'>Error: Something went wrong</h2></div></body></html>";
            var exception = new Exception(htmlPage);
            var args = new CommandLineArgs();

            // Act
            command.LogCliError(exception, args);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Error: Something went wrong"),
                Times.Once
            );
        }

        [Fact]
        public void LogCliError_ShouldLogPlainErrorMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginCommand>>();
            var authServiceMock = new Mock<AuthService>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var remoteServiceExceptionHandlerMock = new Mock<IRemoteServiceExceptionHandler>();

            var command = new LoginCommand(authServiceMock.Object, cancellationTokenProviderMock.Object, remoteServiceExceptionHandlerMock.Object)
            {
                Logger = loggerMock.Object
            };
            var exception = new Exception("An unexpected error occurred");
            var args = new CommandLineArgs();

            // Act
            command.LogCliError(exception, args);

            // Assert
            loggerMock.Verify(
                x => x.LogError("An unexpected error occurred"),
                Times.Once
            );
        }
    }
}
