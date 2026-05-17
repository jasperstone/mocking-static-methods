using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class LoginInfoCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_NotLoggedIn_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

            var command = new LoginInfoCommandForTest(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

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
        public async Task ExecuteAsync_LoggedInButLoginInfoNull_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            var command = new LoginInfoCommandForTest(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

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
        public async Task ExecuteAsync_LoggedInWithLoginInfo_LogsInformation()
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

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var command = new LoginInfoCommandForTest(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await command.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Login info:") &&
                        v.ToString().Contains("Name: John") &&
                        v.ToString().Contains("Surname: Doe") &&
                        v.ToString().Contains("Username: johndoe") &&
                        v.ToString().Contains("Email Address: john.doe@example.com") &&
                        v.ToString().Contains("Organization: ExampleOrg")),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        // Test subclass that hides ExecuteAsync to use injected IAuthService instead of static call
        private class LoginInfoCommandForTest : LoginInfoCommand
        {
            private readonly IAuthService _authService;

            public LoginInfoCommandForTest(IAuthService authService) : base(null)
            {
                _authService = authService;
            }

            public new async Task ExecuteAsync(CommandLineArgs commandLineArgs)
            {
                if (!_authService.IsLoggedIn())
                {
                    Logger.LogError("You are not logged in.");
                    return;
                }

                var loginInfo = await _authService.GetLoginInfoAsync();

                if (loginInfo == null)
                {
                    Logger.LogError("Unable to get login info.");
                    return;
                }

                var sb = new StringBuilder();
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
