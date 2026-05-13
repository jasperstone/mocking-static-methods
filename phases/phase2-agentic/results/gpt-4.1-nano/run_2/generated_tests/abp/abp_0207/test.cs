using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_When_LoggedInAndLoginInfoIsNotNull()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loginInfoMock = new
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john@example.com",
                Organization = "Org"
            };

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfoMock);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Login info:") && s.Contains("Name: John"))),
                Times.Once);
        }

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

            // Act
            await command.ExecuteAsync(null);

            // Assert
            loggerMock.Verify(x => x.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_LoginInfoIsNull()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((object)null);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            loggerMock.Verify(x => x.LogError("Unable to get login info."), Times.Once);
        }

        [Fact]
        public void GetUsageInfo_Should_Return_CorrectString()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var command = new LoginInfoCommand(authServiceMock.Object);

            // Act
            var result = command.GetUsageInfo();

            // Assert
            Assert.Contains("Usage:", result);
            Assert.Contains("abp login-info", result);
            Assert.Contains("https://abp.io/docs/latest/cli", result);
        }

        [Fact]
        public void GetShortDescription_Should_Return_CorrectString()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var command = new LoginInfoCommand(authServiceMock.Object);

            // Act
            var result = LoginInfoCommand.GetShortDescription();

            // Assert
            Assert.Equal("Show your login info.", result);
        }
    }
}
