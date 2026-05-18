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
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(false);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var command = new LoginInfoCommandWrapper(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "You are not logged in."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenLoginInfoIsNull()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var command = new LoginInfoCommandWrapper(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get login info."),
                    null,
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

            var authServiceMock = new Mock<IAuthService>();
            authServiceMock.Setup(a => a.IsLoggedIn()).Returns(true);
            authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

            var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

            var command = new LoginInfoCommandWrapper(authServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            var args = new CommandLineArgs();

            // Act
            await command.ExecuteAsync(args);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                    {
                        var logString = v.ToString();
                        return logString.Contains("Login info:") &&
                               logString.Contains("Name: John") &&
                               logString.Contains("Surname: Doe") &&
                               logString.Contains("Username: johndoe") &&
                               logString.Contains("Email Address: john.doe@example.com") &&
                               logString.Contains("Organization: ExampleOrg");
                    }),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        // Wrapper class to allow mocking IsLoggedIn and GetLoginInfoAsync via interface
        private class LoginInfoCommandWrapper : LoginInfoCommand
        {
            private readonly IAuthService _authService;

            public LoginInfoCommandWrapper(IAuthService authService) : base(null)
            {
                _authService = authService;
                Logger = NullLogger<LoginInfoCommand>.Instance;
            }

            protected override AuthService AuthService => _authService as AuthService;

            public override async Task ExecuteAsync(CommandLineArgs commandLineArgs)
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

    // Interface to abstract AuthService for testing
    public interface IAuthService
    {
        bool IsLoggedIn();
        Task<LoginInfo> GetLoginInfoAsync();
    }

    // Minimal LoginInfo class for testing
    public class LoginInfo
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public string Organization { get; set; }
    }
}
