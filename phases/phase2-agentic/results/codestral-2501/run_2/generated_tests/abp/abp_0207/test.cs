using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Json;
using Volo.Abp.Threading;
using Xunit;

public class LoginInfoCommandTests
{
    private readonly Mock<ILogger<LoginInfoCommand>> _loggerMock;
    private readonly Mock<AuthService> _authServiceMock;
    private readonly LoginInfoCommand _command;

    public LoginInfoCommandTests()
    {
        _loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        _authServiceMock = new Mock<AuthService>(
            Mock.Of<IIdentityModelAuthenticationService>(),
            _loggerMock.Object,
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<RemoteServiceExceptionHandler>(),
            Mock.Of<IJsonSerializer>()
        );
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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You are not logged in.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get login info.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LoggedInAndHasLoginInfo_LogsInformation()
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
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login info:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
