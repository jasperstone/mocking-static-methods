using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Xunit;
using Volo.Abp.Cli.Auth;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var mockAuthService = new Mock<AuthService>(
            Mock.Of<IIdentityModelAuthenticationService>(),
            Mock.Of<ILogger<AuthService>>(),
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
            Mock.Of<Volo.Abp.Cli.Http.RemoteServiceExceptionHandler>(),
            Mock.Of<Volo.Abp.Json.IJsonSerializer>()
        );
        
        mockAuthService.Setup(x => x.IsLoggedIn()).Returns(false);

        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("You are not logged in.") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogError_When_LoginInfo_Is_Null()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var mockAuthService = new Mock<AuthService>(
            Mock.Of<IIdentityModelAuthenticationService>(),
            Mock.Of<ILogger<AuthService>>(),
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
            Mock.Of<Volo.Abp.Cli.Http.RemoteServiceExceptionHandler>(),
            Mock.Of<Volo.Abp.Json.IJsonSerializer>()
        );
        
        mockAuthService.Setup(x => x.IsLoggedIn()).Returns(true);
        mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };

        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("Unable to get login info.") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetUsageInfo_Should_Return_Usage_Text()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>(
            Mock.Of<IIdentityModelAuthenticationService>(),
            Mock.Of<ILogger<AuthService>>(),
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.Cli.Http.CliHttpClientFactory>(),
            Mock.Of<Volo.Abp.Cli.Http.RemoteServiceExceptionHandler>(),
            Mock.Of<Volo.Abp.Json.IJsonSerializer>()
        );

        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = NullLogger<LoginInfoCommand>.Instance
        };

        // Act
        var usageInfo = command.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", usageInfo);
        Assert.Contains("abp login-info", usageInfo);
    }
}
