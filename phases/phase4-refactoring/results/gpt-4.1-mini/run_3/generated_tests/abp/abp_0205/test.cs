using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class LoginInfoCommandTests
{
    private class TestableLoginInfoCommand : LoginInfoCommand
    {
        private readonly bool _isLoggedIn;

        public TestableLoginInfoCommand(AuthService authService, bool isLoggedIn) : base(authService)
        {
            _isLoggedIn = isLoggedIn;
        }

        // Hide static call with instance method for testing
        protected new bool IsLoggedIn()
        {
            return _isLoggedIn;
        }

        public override async Task ExecuteAsync(CommandLineArgs commandLineArgs)
        {
            if (!IsLoggedIn())
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

    [Fact]
    public async Task ExecuteAsync_LogsError_WhenNotLoggedIn()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

        var command = new TestableLoginInfoCommand(authServiceMock.Object, false)
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
                It.IsAny<System.Exception>(),
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsError_WhenLoginInfoIsNull()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>(null, null, null, null, null, null);
        authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

        var command = new TestableLoginInfoCommand(authServiceMock.Object, true)
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
        authServiceMock.Setup(a => a.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();

        var command = new TestableLoginInfoCommand(authServiceMock.Object, true)
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
}
