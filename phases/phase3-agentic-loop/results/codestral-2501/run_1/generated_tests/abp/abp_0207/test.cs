using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<LoginInfoCommand>> _loggerMock;
        private readonly LoginInfoCommand _command;

        public LoginInfoCommandTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<LoginInfoCommand>>();
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
            await _command.ExecuteAsync(new Volo.Abp.Cli.Args.CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You are not logged in.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoginInfoNull_LogsError()
        {
            // Arrange
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((Volo.Abp.Cli.Auth.LoginInfo)null);

            // Act
            await _command.ExecuteAsync(new Volo.Abp.Cli.Args.CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get login info.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ValidLoginInfo_LogsInformation()
        {
            // Arrange
            var loginInfo = new Volo.Abp.Cli.Auth.LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "Abp"
            };
            _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
            _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var expectedLogMessage = new StringBuilder()
                .AppendLine("")
                .AppendLine("Login info:")
                .AppendLine("Name: John")
                .AppendLine("Surname: Doe")
                .AppendLine("Username: johndoe")
                .AppendLine("Email Address: john.doe@example.com")
                .AppendLine("Organization: Abp")
                .ToString();

            // Act
            await _command.ExecuteAsync(new Volo.Abp.Cli.Args.CommandLineArgs());

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedLogMessage)),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
