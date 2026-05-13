using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
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
        public async Task ExecuteAsync_NotLoggedIn_LogsError()
        {
            // Arrange
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(false);

            // Act
            await _command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(x => x.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoggedIn_LogsLoginInfo()
        {
            // Arrange
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Example Org"
            };
            _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var expectedLogMessage = new StringBuilder()
                .AppendLine("")
                .AppendLine("Login info:")
                .AppendLine("Name: John")
                .AppendLine("Surname: Doe")
                .AppendLine("Username: johndoe")
                .AppendLine("Email Address: john.doe@example.com")
                .AppendLine("Organization: Example Org")
                .ToString();

            // Act
            await _command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(x => x.LogInformation(expectedLogMessage), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_UnableToGetLoginInfo_LogsError()
        {
            // Arrange
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            // Act
            await _command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(x => x.LogError("Unable to get login info."), Times.Once);
        }

        [Fact]
        public void GetUsageInfo_ReturnsCorrectUsageInfo()
        {
            // Arrange
            var expectedUsageInfo = new StringBuilder()
                .AppendLine("")
                .AppendLine("Usage:")
                .AppendLine("  abp login-info")
                .AppendLine("")
                .AppendLine("See the documentation for more info: https://abp.io/docs/latest/cli")
                .ToString();

            // Act
            var usageInfo = _command.GetUsageInfo();

            // Assert
            Assert.Equal(expectedUsageInfo, usageInfo);
        }

        [Fact]
        public void GetShortDescription_ReturnsCorrectDescription()
        {
            // Act
            var description = LoginInfoCommand.GetShortDescription();

            // Assert
            Assert.Equal("Show your login info.", description);
        }
    }
}
