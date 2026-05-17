using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Moq;
using System.IO;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_LoginInfoIsNull()
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

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogError("Unable to get login info."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogLoginInfo_When_LoginInfoIsAvailable()
        {
            // Arrange
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john@example.com",
                Organization = "ExampleOrg"
            };

            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Login info:"))), Times.Once);
        }
    }

    // Dummy class to satisfy the code dependencies
    public class LoginInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string Organization { get; set; }
    }
}
