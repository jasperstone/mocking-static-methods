using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;

public interface IAuthServiceWrapper
{
    bool IsLoggedIn();
    Task<LoginInfo> GetLoginInfoAsync();
}

public class AuthServiceWrapper : IAuthServiceWrapper
{
    private readonly AuthService _authService;

    public AuthServiceWrapper(AuthService authService)
    {
        _authService = authService;
    }

    public bool IsLoggedIn()
    {
        return AuthService.IsLoggedIn();
    }

    public Task<LoginInfo> GetLoginInfoAsync()
    {
        return _authService.GetLoginInfoAsync();
    }
}

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WhenLoggedIn_LogsLoginInfo()
    {
        // Arrange
        var mockAuthServiceWrapper = new Mock<IAuthServiceWrapper>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(mockAuthServiceWrapper.Object)
        {
            Logger = mockLogger.Object
        };

        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "Example Org"
        };

        mockAuthServiceWrapper.Setup(x => x.IsLoggedIn()).Returns(true);
        mockAuthServiceWrapper.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Login info:") &&
                                    s.Contains("Name: John") &&
                                    s.Contains("Surname: Doe") &&
                                    s.Contains("Username: johndoe") &&
                                    s.Contains("Email Address: john.doe@example.com") &&
                                    s.Contains("Organization: Example Org")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotLoggedIn_LogsError()
    {
        // Arrange
        var mockAuthServiceWrapper = new Mock<IAuthServiceWrapper>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(mockAuthServiceWrapper.Object)
        {
            Logger = mockLogger.Object
        };

        mockAuthServiceWrapper.Setup(x => x.IsLoggedIn()).Returns(false);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("You are not logged in.")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoginInfoIsNull_LogsError()
    {
        // Arrange
        var mockAuthServiceWrapper = new Mock<IAuthServiceWrapper>();
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(mockAuthServiceWrapper.Object)
        {
            Logger = mockLogger.Object
        };

        mockAuthServiceWrapper.Setup(x => x.IsLoggedIn()).Returns(true);
        mockAuthServiceWrapper.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("Unable to get login info.")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
