using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
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
            _loggerMock.Verify(
                x => x.LogError("You are not logged in."),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoggedInButNoLoginInfo_LogsError()
        {
            // Arrange
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            // Act
            await _command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                x => x.LogError("Unable to get login info."),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoggedInAndHasLoginInfo_LogsLoginInfo()
        {
            // Arrange
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Example Org"
            };
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            // Act
            await _command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Login info:"))),
                Times.Once);
        }
    }
}
