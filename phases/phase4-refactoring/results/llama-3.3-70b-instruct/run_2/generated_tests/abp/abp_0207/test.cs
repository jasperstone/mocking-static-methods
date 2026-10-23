using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Core;
using Volo.Abp.Cli.Core.Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LoggedIn_LoginInfoLogged()
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
        public async Task ExecuteAsync_NotLoggedIn_LoginErrorLogged()
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
        public async Task ExecuteAsync_LoginInfoNull_LoginErrorLogged()
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
