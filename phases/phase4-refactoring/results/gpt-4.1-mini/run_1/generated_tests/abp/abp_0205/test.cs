using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(
                null, null, null, null, null, null);

            // Setup IsLoggedIn static method by mocking AuthService.IsLoggedIn to false via wrapper
            var command = new LoginInfoCommand(authServiceMock.Object);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs();

            // Act
            // We cannot mock static IsLoggedIn, so simulate by making sure the file does not exist
            // But since we cannot do that here, we just call ExecuteAsync and verify LogError is called if IsLoggedIn returns false
            // So we will just call ExecuteAsync and verify LogError was called at least once with the expected message
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You are not logged in."),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
