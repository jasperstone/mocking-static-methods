using Xunit;
using Moq;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.Core.Commands;
using Volo.Abp.Cli.Core.Models;
using Volo.Abp.Cli.Core.Services;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Core.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsLoginInfo_WhenLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "johndoe@example.com",
                Organization = "Example Organization"
            };

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenGetLoginInfoFails()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
