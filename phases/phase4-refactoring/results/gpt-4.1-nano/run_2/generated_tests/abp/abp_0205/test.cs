using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using System.IO;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new Volo.Abp.Cli.Args.CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }
    }
}
