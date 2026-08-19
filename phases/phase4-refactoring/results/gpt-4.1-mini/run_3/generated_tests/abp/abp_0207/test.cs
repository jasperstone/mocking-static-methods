using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class LoginInfoCommandTests
{
    private class FakeAuthService : IAuthService
    {
        private readonly bool _isLoggedIn;
        private readonly LoginInfo _loginInfo;

        public FakeAuthService(bool isLoggedIn, LoginInfo loginInfo)
        {
            _isLoggedIn = isLoggedIn;
            _loginInfo = loginInfo;
        }

        public Task<LoginInfo> GetLoginInfoAsync()
        {
            return Task.FromResult(_loginInfo);
        }

        public Task LoginAsync(string userName, string password, string organizationName = null)
        {
            throw new System.NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> CheckMultipleOrganizationsAsync(string username)
        {
            throw new System.NotImplementedException();
        }

        // We add an extension method for IsLoggedIn in the test namespace to simulate the static method
        public bool IsLoggedIn()
        {
            return _isLoggedIn;
        }
    }

    private class TestLoginInfoCommand : LoginInfoCommand
    {
        private readonly IAuthService _authService;

        public TestLoginInfoCommand(IAuthService authService)
            : base(new AuthServiceStub())
        {
            _authService = authService;
            Logger = NullLogger<LoginInfoCommand>.Instance;
        }

        public override async Task ExecuteAsync(CommandLineArgs commandLineArgs)
        {
            if (!(_authService as FakeAuthService)?.IsLoggedIn() ?? false)
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

    private class AuthServiceStub : AuthService
    {
        public AuthServiceStub() : base(null, null, null, null, null, null) { }
    }

    [Fact]
    public async Task ExecuteAsync_NotLoggedIn_LogsError()
    {
        var authService = new FakeAuthService(false, null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new TestLoginInfoCommand(authService)
        {
            Logger = loggerMock.Object
        };

        await command.ExecuteAsync(new CommandLineArgs());

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
    public async Task ExecuteAsync_LoginInfoNull_LogsError()
    {
        var authService = new FakeAuthService(true, null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new TestLoginInfoCommand(authService)
        {
            Logger = loggerMock.Object
        };

        await command.ExecuteAsync(new CommandLineArgs());

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
    public async Task ExecuteAsync_ValidLoginInfo_LogsInformation()
    {
        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "ExampleOrg"
        };
        var authService = new FakeAuthService(true, loginInfo);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new TestLoginInfoCommand(authService)
        {
            Logger = loggerMock.Object
        };

        await command.ExecuteAsync(new CommandLineArgs());

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
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }
}
