using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var mockAuthService = new Mock<AuthService>();
        mockAuthService.SetupSequence(x => x.IsLoggedIn())
            .Returns(false);
        
        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };
        
        var args = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You are not logged in.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_LogError_When_LoginInfo_Null()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var mockAuthService = new Mock<AuthService>();
        mockAuthService.SetupSequence(x => x.IsLoggedIn())
            .Returns(true);
        mockAuthService.SetupSequence(x => x.GetLoginInfoAsync())
            .ReturnsAsync((LoginInfo)null);
        
        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };
        
        var args = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get login info.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Should_LogInformation_When_LoggedIn_WithValidInfo()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        var mockAuthService = new Mock<AuthService>();
        mockAuthService.SetupSequence(x => x.IsLoggedIn())
            .Returns(true);
        mockAuthService.SetupSequence(x => x.GetLoginInfoAsync())
            .ReturnsAsync(new LoginInfo 
            { 
                Name = "John", 
                Surname = "Doe", 
                Username = "johndoe", 
                EmailAddress = "john@example.com", 
                Organization = "Acme Corp" 
            });
        
        var command = new LoginInfoCommand(mockAuthService.Object)
        {
            Logger = mockLogger.Object
        };
        
        var args = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Login info:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }

    [Fact]
    public void GetUsageInfo_Should_Return_UsageText()
    {
        // Arrange
        var mockAuthService = new Mock<AuthService>();
        var command = new LoginInfoCommand(mockAuthService.Object);

        // Act
        var result = command.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", result);
        Assert.Contains("abp login-info", result);
    }
}
