using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
        {
            // Arrange
            var mockAuthService = new Mock<AuthService>();
            mockAuthService.Setup(service => service.IsLoggedIn()).Returns(false);

            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
            var loginInfoCommand = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await loginInfoCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(logger => logger.LogError("You are not logged in."), Times.Once);
        }
    }
}
