using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandTests
    {
        [Fact]
        public async Task LogError_Should_LogError_When_MessageContainsInvalidUsername()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LoginCommand>>();
            var loginCommand = new LoginCommand(
                authService: null,
                cancellationTokenProvider: null,
                remoteServiceExceptionHandler: null);
            loginCommand.Logger = mockLogger.Object;

            var exception = new Exception("Invalid username or password");
            var args = new CommandLineArgs();

            // Act
            loginCommand.LogCliError(exception, args);

            // Assert
            mockLogger.Verify(x => x.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task LogError_Should_LogError_When_MessageContainsRequiresTwoFactor()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LoginCommand>>();
            var loginCommand = new LoginCommand(
                authService: null,
                cancellationTokenProvider: null,
                remoteServiceExceptionHandler: null);
            loginCommand.Logger = mockLogger.Object;

            var exception = new Exception("RequiresTwoFactor");
            var args = new CommandLineArgs();

            // Act
            loginCommand.LogCliError(exception, args);

            // Assert
            mockLogger.Verify(x => x.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task LogError_Should_LogError_FromHtmlPage_When_MessageContainsErrorPage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LoginCommand>>();
            var loginCommand = new LoginCommand(
                authService: null,
                cancellationTokenProvider: null,
                remoteServiceExceptionHandler: null);
            loginCommand.Logger = mockLogger.Object;

            string htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Error occurred</h2></div>";
            var exception = new Exception(htmlError);
            var args = new CommandLineArgs();

            // Act
            loginCommand.LogCliError(exception, args);

            // Assert
            mockLogger.Verify(x => x.LogError("Error occurred"), Times.Once);
        }

        [Fact]
        public async Task LogError_Should_LogError_Message_When_NoSpecificConditionMatches()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LoginCommand>>();
            var loginCommand = new LoginCommand(
                authService: null,
                cancellationTokenProvider: null,
                remoteServiceExceptionHandler: null);
            loginCommand.Logger = mockLogger.Object;

            var exception = new Exception("Some other error");
            var args = new CommandLineArgs();

            // Act
            loginCommand.LogCliError(exception, args);

            // Assert
            mockLogger.Verify(x => x.LogError("Some other error"), Times.Once);
        }
    }
}
