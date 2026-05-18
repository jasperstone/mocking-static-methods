using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    private readonly Mock<ILogger<LoginInfoCommand>> _loggerMock;
    private readonly Mock<AuthService> _authServiceMock;
    private readonly LoginInfoCommand _command;
    private readonly CommandLineArgs _args;

    public LoginInfoCommandTests()
    {
        _loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        Mock<ILogger<AuthService>> authLoggerMock = new();
        _authServiceMock = new Mock<AuthService>(
            Mock.Of<Volo.Abp.IdentityModel.IIdentityModelAuthenticationService>(),
            authLoggerMock.Object,
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<RemoteServiceExceptionHandler>(),
            Mock.Of<Volo.Abp.Json.IJsonSerializer>()
        );
        
        _command = new LoginInfoCommand(_authServiceMock.Object)
        {
            Logger = _loggerMock.Object
        };
        
        _args = CommandLineArgs.Empty();
    }

    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(false);

        // Act
        await _command.ExecuteAsync(_args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You are not logged in.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_LogError_When_LoginInfo_Is_Null()
    {
        // Arrange
        _authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
        _authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null!);

        // Act
        await _command.ExecuteAsync(_args);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to get login info.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
