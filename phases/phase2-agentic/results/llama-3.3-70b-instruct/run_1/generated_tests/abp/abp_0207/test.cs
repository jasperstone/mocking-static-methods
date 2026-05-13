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
        public async Task ExecuteAsync_LoggedIn_LogsLoginInfo()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "johndoe@example.com",
                Organization = "Example Organization"
            };
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Login info:"))), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_NotLoggedIn_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoginInfoNull_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("Unable to get login info."), Times.Once);
        }
    }

    public class LoginInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string Organization { get; set; }
    }
}
