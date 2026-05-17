using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LoggedIn_LogsLoginInfo()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;
            System.IO.File.WriteAllText("access_token", "token");

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Login info:"))), Times.Once);
            System.IO.File.Delete("access_token");
        }

        [Fact]
        public async Task ExecuteAsync_NotLoggedIn_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("You are not logged in."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LoginInfoNull_LogsError()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>();
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var command = new LoginInfoCommand(authServiceMock.Object);
            command.Logger = loggerMock.Object;
            System.IO.File.WriteAllText("access_token", "token");

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogError("Unable to get login info."), Times.Once);
            System.IO.File.Delete("access_token");
        }
    }
}
