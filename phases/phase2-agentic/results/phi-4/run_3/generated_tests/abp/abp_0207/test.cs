using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Models;
using Volo.Abp.Cli.Services;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenLoggedIn_ShouldLogLoginInfo()
        {
            // Arrange
            var mockAuthService = new Mock<AuthService>();
            mockAuthService.Setup(service => service.IsLoggedIn()).Returns(true);
            mockAuthService.Setup(service => service.GetLoginInfoAsync())
                .ReturnsAsync(new LoginInfo
                {
                    Name = "John",
                    Surname = "Doe",
                    Username = "johndoe",
                    EmailAddress = "john.doe@example.com",
                    Organization = "Example Org"
                });

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
                    It.Is<string>(s => s.Contains("Login info:") &&
                                      s.Contains("Name: John") &&
                                      s.Contains("Surname: Doe") &&
                                      s.Contains("Username: johndoe") &&
                                      s.Contains("Email Address: john.doe@example.com") &&
                                      s.Contains("Organization: Example Org")),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
