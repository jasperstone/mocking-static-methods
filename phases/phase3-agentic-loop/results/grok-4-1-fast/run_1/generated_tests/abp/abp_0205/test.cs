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

public class LoginInfoCommandTests : IDisposable
{
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<ILogger<LoginInfoCommand>> _mockLogger;
    private readonly LoginInfoCommand _command;
    private readonly string _originalTokenPathContent;

    public LoginInfoCommandTests()
    {
        _mockAuthService = new Mock<AuthService>();
        _mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };

        // Backup and clear token file to simulate not logged in
        _originalTokenPathContent = File.Exists(AuthService.CliPaths.AccessToken) 
            ? File.ReadAllText(AuthService.CliPaths.AccessToken) 
            : null;
        if (File.Exists(AuthService.CliPaths.AccessToken))
        {
            File.Delete(AuthService.CliPaths.AccessToken);
        }
    }

    public void Dispose()
    {
        // Restore original token file state
        if (_originalTokenPathContent == null)
        {
            if (File.Exists(AuthService.CliPaths.AccessToken))
            {
                File.Delete(AuthService.CliPaths.AccessToken);
            }
        }
        else
        {
            File.WriteAllText(AuthService.CliPaths.AccessToken, _originalTokenPathContent);
        }
    }

    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange - token file doesn't exist (AuthService.IsLoggedIn() returns false)
        var args = new CommandLineArgs();

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You are not logged in.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogError_When_LoginInfo_Is_Null()
    {
        // Arrange - create token file to simulate logged in
        File.WriteAllText(AuthService.CliPaths.AccessToken, "fake-token");
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        var args = new CommandLineArgs();

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to get login info.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_LogInformation_When_LoginInfo_Successful()
    {
        // Arrange - create token file to simulate logged in
        File.WriteAllText(AuthService.CliPaths.AccessToken, "fake-token");
        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john@example.com",
            Organization = "Acme Corp"
        };
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
        var args = new CommandLineArgs();

        // Act
        await _command.ExecuteAsync(args);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login info:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
