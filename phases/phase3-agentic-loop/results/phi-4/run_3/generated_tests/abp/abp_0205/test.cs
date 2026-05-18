using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
        {
            // Arrange
            var authServiceWrapperMock = new Mock<IAuthServiceWrapper>();
            authServiceWrapperMock.Setup(a => a.IsLoggedIn()).Returns(false);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var loginInfoCommand = new LoginInfoCommand(authServiceWrapperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await loginInfoCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }
    }
}
