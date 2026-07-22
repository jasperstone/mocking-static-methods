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
        public async Task ExecuteAsync_Should_LogLoginInfo_When_LoggedIn()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john@example.com",
                Organization = "Acme Corp"
            };

            mockAuthService.Setup(a => a.IsLoggedIn()).Returns(true);
            mockAuthService.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var command = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Login info:") && s.Contains("Name: John"))),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

            mockAuthService.Setup(a => a.IsLoggedIn()).Returns(false);

            var command = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            mockLogger.Verify(x => x.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_LoginInfoIsNull()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

            mockAuthService.Setup(a => a.IsLoggedIn()).Returns(true);
            mockAuthService.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            var command = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            mockLogger.Verify(x => x.LogError("Unable to get login info."), Times.Once);
        }

        [Fact]
        public void GetUsageInfo_Should_Return_Correct_Info()
        {
            // Arrange
            var command = new LoginInfoCommand(Mock.Of<IAuthService>());

            // Act
            var usageInfo = command.GetUsageInfo();

            // Assert
            Assert.Contains("abp login-info", usageInfo);
            Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
        }

        [Fact]
        public static void GetShortDescription_Should_Return_CorrectString()
        {
            // Act
            var description = LoginInfoCommand.GetShortDescription();

            // Assert
            Assert.Equal("Show your login info.", description);
        }
    }
}
