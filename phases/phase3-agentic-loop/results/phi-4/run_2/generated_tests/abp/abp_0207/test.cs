using Moq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenLoggedInAndLoginInfoIsNotNull_LogsInformation()
        {
            // Arrange
            var mockAuthServiceWrapper = new Mock<IAuthServiceWrapper>();
            mockAuthServiceWrapper.Setup(service => service.IsLoggedIn()).Returns(true);
            mockAuthServiceWrapper.Setup(service => service.GetLoginInfoAsync())
                .ReturnsAsync(new LoginInfo
                {
                    Name = "John",
                    Surname = "Doe",
                    Username = "johndoe",
                    EmailAddress = "john.doe@example.com",
                    Organization = "Example Org"
                });

            var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(mockAuthServiceWrapper.Object)
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
