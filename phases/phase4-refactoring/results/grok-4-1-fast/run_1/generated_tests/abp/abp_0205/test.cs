using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<LoginInfoCommand>> _loggerMock;
    private readonly LoginInfoCommand _command;
    private readonly CommandLineArgs _commandLineArgs;

    public LoginInfoCommandTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        _command = new LoginInfoCommand(_authServiceMock.Object)
        {
            Logger = _loggerMock.Object
        };
        _commandLineArgs = new CommandLineArgs();
    }

    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(false);

        // Act
        await _command.ExecuteAsync(_commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("You are not logged in.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _authServiceMock.Verify(x => x.GetLoginInfoAsync(), Times.Never);
    }

    [Fact]
    public async Task Should_LogError_When_LoginInfo_Is_Null()
    {
        // Arrange
        _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
        _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        // Act
        await _command.ExecuteAsync(_commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Unable to get login info.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogInformation_When_Successful()
    {
        // Arrange
        var loginInfo = new LoginInfo
        {
            Name = "Test",
            Surname = "User",
            Username = "testuser",
            EmailAddress = "test@example.com",
            Organization = "TestOrg"
        };

        _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
        _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

        // Act
        await _command.ExecuteAsync(_commandLineArgs);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Login info:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
