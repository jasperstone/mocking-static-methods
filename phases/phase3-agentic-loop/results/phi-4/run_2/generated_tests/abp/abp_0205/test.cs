using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.Args; // Added using directive for CommandLineArgs

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var authServiceMock = new Mock<AuthService>();

            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs(""));

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }
    }
}
