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
        public async Task ExecuteAsync_LoggedIn_LoginInfoLogged()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfoCommand = new LoginInfoCommand(authServiceMock.Object);
            loginInfoCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "johndoe@example.com",
                Organization = "Example Organization"
            });

            // Act
            await loginInfoCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_NotLoggedIn_LoginErrorLogged()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfoCommand = new LoginInfoCommand(authServiceMock.Object);
            loginInfoCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            // Act
            await loginInfoCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoginInfoNull_LoginErrorLogged()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfoCommand = new LoginInfoCommand(authServiceMock.Object);
            loginInfoCommand.Logger = loggerMock.Object;

            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            // Act
            await loginInfoCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
