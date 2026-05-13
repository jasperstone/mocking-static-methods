using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfo = new
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john@example.com",
                Organization = "Org"
            };

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

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
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

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
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((object)null);

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
        public async Task ExecuteAsync_Should_Call_LogInformation_With_Correct_Content()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfo = new
            {
                Name = "Alice",
                Surname = "Smith",
                Username = "alicesmith",
                EmailAddress = "alice@example.com",
                Organization = "Acme"
            };

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            string capturedLog = null;
            loggerMock.Setup(x => x.LogInformation(It.IsAny<string>()))
                      .Callback<string>(msg => capturedLog = msg);

            // Act
            await command.ExecuteAsync(null);

            // Assert
            Assert.NotNull(capturedLog);
            Assert.Contains("Name: Alice", capturedLog);
            Assert.Contains("Surname: Smith", capturedLog);
            Assert.Contains("Username: alicesmith", capturedLog);
            Assert.Contains("Email Address: alice@example.com", capturedLog);
            Assert.Contains("Organization: Acme", capturedLog);
        }
    }
}
