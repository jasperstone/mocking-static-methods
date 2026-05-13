using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

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

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(
                x => x.LogError("You are not logged in."),
                Times.Once);
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

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(
                x => x.LogError("Unable to get login info."),
                Times.Once);
        }
    }
}
