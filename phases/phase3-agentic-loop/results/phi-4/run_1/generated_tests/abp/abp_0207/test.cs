using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenLoggedInAndLoginInfoNotNull_LogsInformation()
        {
            // Arrange
            var mockAuthService = new Mock<AuthService>();
            mockAuthService.Setup(service => service.GetLoginInfoAsync())
                .ReturnsAsync(new LoginInfo
                {
                    Name = "John",
                    Surname = "Doe",
                    Username = "johndoe",
                    EmailAddress = "john.doe@example.com",
                    Organization = "Example Org"
                });

            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(mockAuthService.Object)
            {
                Logger = mockLogger.Object
            };

            // Simulate being logged in
            File.WriteAllText("mockAccessTokenPath", "mockAccessToken");

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

            // Clean up
            File.Delete("mockAccessTokenPath");
        }
    }
}
