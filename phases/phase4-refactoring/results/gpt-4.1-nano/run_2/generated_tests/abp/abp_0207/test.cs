using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LoggedInAndLoginInfoAvailable_ShouldLogLoginInfo()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var loginInfo = new
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Acme Corp"
            };

            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var command = new LoginInfoCommand(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(null);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Login info:") && s.Contains("Name: John"))),
                Times.Once);
        }
    }
}
