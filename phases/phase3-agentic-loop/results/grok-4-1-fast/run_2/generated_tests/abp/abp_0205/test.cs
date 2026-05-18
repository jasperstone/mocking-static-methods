using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;
using System.IO;

namespace Volo.Abp.Cli.Tests.Commands;

public class LoginInfoCommandTests : IDisposable
{
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<ILogger<LoginInfoCommand>> _mockLogger;
    private LoginInfoCommand _command;
    private readonly string _tempPath;

    public LoginInfoCommandTests()
    {
        _mockAuthService = new Mock<AuthService>();
        _mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        _tempPath = Path.Combine(Path.GetTempPath(), "abp-cli", "accesstoken.tmp");
        Directory.CreateDirectory(Path.GetDirectoryName(_tempPath)!);
    }

    public void Dispose()
    {
        if (File.Exists(_tempPath))
        {
            File.Delete(_tempPath);
        }
        if (Directory.Exists(Path.GetDirectoryName(_tempPath)!))
        {
            Directory.Delete(Path.GetDirectoryName(_tempPath)!, true);
        }
    }

    [Fact]
    public async void Should_LogError_When_NotLoggedIn()
    {
        // Arrange - no token file exists, so IsLoggedIn() returns false
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _mockLogger.Verify(
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
    public async void Should_LogError_When_LoginInfo_Is_Null()
    {
        // Arrange
        File.WriteAllText(_tempPath, "fake-token"); // Makes IsLoggedIn() return true
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _mockLogger.Verify(
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

    [Fact]
    public async void Should_LogInformation_When_LoginInfo_Successful()
    {
        // Arrange
        File.WriteAllText(_tempPath, "fake-token");
        var loginInfo = new LoginInfo 
        { 
            Name = "John", 
            Surname = "Doe", 
            Username = "johndoe", 
            EmailAddress = "john@example.com", 
            Organization = "Acme Corp" 
        };
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login info:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once
        );
    }
}
