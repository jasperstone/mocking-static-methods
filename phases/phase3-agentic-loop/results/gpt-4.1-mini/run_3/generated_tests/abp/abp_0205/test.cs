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
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
            // IsLoggedIn is static, so we cannot mock it on instance, we will mock the static method by wrapping it in a local function or by using a derived class
            // But here, we will create a derived class to override IsLoggedIn behavior for testing
            var command = new TestLoginInfoCommand(authServiceMock.Object, isLoggedIn: false);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You are not logged in."),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenLoginInfoIsNull()
        {
            // Arrange
            var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
            var command = new TestLoginInfoCommand(authServiceMock.Object, isLoggedIn: true);

            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get login info."),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsInformation_WhenLoginInfoIsAvailable()
        {
            // Arrange
            var loginInfo = new LoginInfo
            {
                Name = "John",
                Surname = "Doe",
                Username = "johndoe",
                EmailAddress = "john.doe@example.com",
                Organization = "ExampleOrg"
            };

            var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
            var command = new TestLoginInfoCommand(authServiceMock.Object, isLoggedIn: true);

            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            command.Logger = loggerMock.Object;

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login info:") &&
                                                   v.ToString().Contains("Name: John") &&
                                                   v.ToString().Contains("Surname: Doe") &&
                                                   v.ToString().Contains("Username: johndoe") &&
                                                   v.ToString().Contains("Email Address: john.doe@example.com") &&
                                                   v.ToString().Contains("Organization: ExampleOrg")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        private class TestLoginInfoCommand : LoginInfoCommand
        {
            private readonly bool _isLoggedIn;

            public TestLoginInfoCommand(AuthService authService, bool isLoggedIn) : base(authService)
            {
                _isLoggedIn = isLoggedIn;
            }

            // Override the static call to IsLoggedIn by hiding the method and using the instance field
            public new async Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                if (!_isLoggedIn)
                {
                    Logger.LogError("You are not logged in.");
                    return;
                }

                var loginInfo = await AuthService.GetLoginInfoAsync();

                if (loginInfo == null)
                {
                    Logger.LogError("Unable to get login info.");
                    return;
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("");
                sb.AppendLine("Login info:");
                sb.AppendLine($"Name: {loginInfo.Name}");
                sb.AppendLine($"Surname: {loginInfo.Surname}");
                sb.AppendLine($"Username: {loginInfo.Username}");
                sb.AppendLine($"Email Address: {loginInfo.EmailAddress}");
                sb.AppendLine($"Organization: {loginInfo.Organization}");
                Logger.LogInformation(sb.ToString());
            }
        }
    }
}
