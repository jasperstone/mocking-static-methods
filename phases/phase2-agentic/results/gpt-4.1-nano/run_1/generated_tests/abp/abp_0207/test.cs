using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        private readonly Mock<ILogger<LoginInfoCommand>> _loggerMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly LoginInfoCommand _command;

        public LoginInfoCommandTests()
        {
            _loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            _authServiceMock = new Mock<AuthService>();
            _command = new LoginInfoCommand(_authServiceMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
        {
            // Arrange
            _authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

            // Act
            await _command.ExecuteAsync(null);

            // Assert
            _loggerMock.Verify(
                x => x.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogInformation_WithLoginInfo()
        {
            // Arrange
            _authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            var loginInfo = new
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john@example.com",
                Organization = "Acme Corp"
            };
            _authServiceMock.Setup(a => a.GetLoginInfoAsync())
                .ReturnsAsync(loginInfo);

            string loggedMessage = null;
            _loggerMock.Setup(x => x.LogInformation(It.IsAny<string>()))
                .Callback<string>(msg => loggedMessage = msg);

            // Act
            await _command.ExecuteAsync(null);

            // Assert
            Assert.NotNull(loggedMessage);
            Assert.Contains("Login info:", loggedMessage);
            Assert.Contains($"Name: {loginInfo.Name}", loggedMessage);
            Assert.Contains($"Surname: {loginInfo.Surname}", loggedMessage);
            Assert.Contains($"Username: {loginInfo.Username}", loggedMessage);
            Assert.Contains($"Email Address: {loginInfo.EmailAddress}", loggedMessage);
            Assert.Contains($"Organization: {loginInfo.Organization}", loggedMessage);
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_LoginInfoIsNull()
        {
            // Arrange
            _authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            _authServiceMock.Setup(a => a.GetLoginInfoAsync())
                .ReturnsAsync((object)null);

            // Act
            await _command.ExecuteAsync(null);

            // Assert
            _loggerMock.Verify(x => x.LogError("Unable to get login info."), Times.Once);
        }
    }
}
