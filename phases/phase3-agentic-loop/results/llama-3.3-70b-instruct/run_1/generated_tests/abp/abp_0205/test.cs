using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenGetLoginInfoFails()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInfo_WhenGetLoginInfoSucceeds()
        {
            // Arrange
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "johndoe@example.com",
                Organization = "Example Organization"
            };

            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
