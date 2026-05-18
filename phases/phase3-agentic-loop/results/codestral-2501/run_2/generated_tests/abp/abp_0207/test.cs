using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Internal;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(x => x.IsLoggedIn()).Returns(false);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(
                x => x.LogError("You are not logged in."),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenLoginInfoIsNull_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(
                x => x.LogError("Unable to get login info."),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenLoginInfoIsNotNull_LogsInformation()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Example Org"
            };
            authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            var expectedLogMessage = new StringBuilder()
                .AppendLine("")
                .AppendLine("Login info:")
                .AppendLine("Name: John")
                .AppendLine("Surname: Doe")
                .AppendLine("Username: johndoe")
                .AppendLine("Email Address: john.doe@example.com")
                .AppendLine("Organization: Example Org")
                .ToString();

            loggerMock.Verify(
                x => x.LogInformation(expectedLogMessage),
                Times.Once);
        }
    }
}
