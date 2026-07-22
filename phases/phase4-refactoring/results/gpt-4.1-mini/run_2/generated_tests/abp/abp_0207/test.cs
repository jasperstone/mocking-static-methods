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
    private class FakeAuthService : AuthService
    {
        private readonly bool _isLoggedIn;
        private readonly LoginInfo _loginInfo;

        public FakeAuthService(bool isLoggedIn, LoginInfo loginInfo)
            : base(null, null, null, null, null, null)
        {
            _isLoggedIn = isLoggedIn;
            _loginInfo = loginInfo;
        }

        public new bool IsLoggedIn()
        {
            return _isLoggedIn;
        }

        public override Task<LoginInfo> GetLoginInfoAsync()
        {
            return Task.FromResult(_loginInfo);
        }
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation_WhenLoggedInAndLoginInfoIsNotNull()
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

        var fakeAuthService = new FakeAuthService(true, loginInfo);

        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();

        var command = new LoginInfoCommand(fakeAuthService)
        {
            Logger = mockLogger.Object
        };

        var args = new CommandLineArgs("");

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login info:") && v.ToString().Contains("John") && v.ToString().Contains("Doe") && v.ToString().Contains("johndoe") && v.ToString().Contains("john.doe@example.com") && v.ToString().Contains("ExampleOrg")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }
}
