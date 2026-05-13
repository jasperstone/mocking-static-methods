using Moq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenLoggedInAndLoginInfoIsNotNull_LogsLoginInfo()
        {
            // Arrange
            var mockAuthService = new Mock<AuthService>();
            mockAuthService.Setup(service => service.IsLoggedIn()).Returns(true);
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Example Org"
            };
            mockAuthService.Setup(service => service.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s =>
                        s.Contains("Login info:") &&
                        s.Contains("Name: John") &&
                        s.Contains("Surname: Doe") &&
                        s.Contains("Username: johndoe") &&
                        s.Contains("Email Address: john.doe@example.com") &&
                        s.Contains("Organization: Example Org")
                    )
                ),
                Times.Once
            );
        }
    }
}
