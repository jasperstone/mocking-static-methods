using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

namespace Volo.Abp.Cli.Tests
{
    public class LoginCommandTests
    {
        private readonly Mock<ILogger<LoginCommand>> _loggerMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly LoginCommand _loginCommand;

        public LoginCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginCommand>>();
            _authServiceMock = new Mock<AuthService>();
            _loginCommand = new LoginCommand(_authServiceMock.Object, null, null)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task HasMultipleOrganizationAndThisNotSpecified_ShouldLogErrorAndReturnTrue()
        {
            // Arrange
            var commandLineArgs = new CommandLineArgs
            {
                Target = "user",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };
            _authServiceMock.Setup(a => a.CheckMultipleOrganizationsAsync(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _loginCommand.HasMultipleOrganizationAndThisNotSpecified(commandLineArgs, null);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("multiple organizations"))), Times.Once);
        }

        [Fact]
        public async Task LogCliError_ShouldLogCorrectMessage_ForInvalidUsernameOrPassword()
        {
            // Arrange
            var ex = new Exception("Invalid username or password");
            var commandLineArgs = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogError("Invalid username or password!"), Times.Once);
        }

        [Fact]
        public async Task LogCliError_ShouldLogCorrectMessage_ForRequiresTwoFactor()
        {
            // Arrange
            var ex = new Exception("RequiresTwoFactor");
            var commandLineArgs = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogError("Two factor authentication is enabled for your account. Please use `abp login --device` command to login."), Times.Once);
        }

        [Fact]
        public async Task LogCliError_ShouldLogHtmlErrorMessage_WhenHtmlContainsErrorPage()
        {
            // Arrange
            var htmlError = "<div class=\"error-page-container\"><h2 class=\"text-danger\">Error occurred</h2></div>";
            var ex = new Exception(htmlError);
            var commandLineArgs = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogError("Error occurred"), Times.Once);
        }

        [Fact]
        public async Task LogCliError_ShouldLogDefaultMessage_WhenNoSpecificErrorMatched()
        {
            // Arrange
            var ex = new Exception("Some other error");
            var commandLineArgs = new CommandLineArgs();

            // Act
            _loginCommand.LogCliError(ex, commandLineArgs);

            // Assert
            _loggerMock.Verify(l => l.LogError("Some other error"), Times.Once);
        }
    }
}
