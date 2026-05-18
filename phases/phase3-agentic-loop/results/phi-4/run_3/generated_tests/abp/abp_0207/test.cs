using Moq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Models;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenLoggedInAndLoginInfoNotNull_LogsInformation()
        {
            // Arrange
            var mockAuthService = new Mock<IAuthService>();
            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Example Org"
            };

            mockAuthService.Setup(s => s.IsLoggedIn()).Returns(true);
            mockAuthService.Setup(s => s.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var command = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            mockLogger.Verify(
                l => l.LogInformation(
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
