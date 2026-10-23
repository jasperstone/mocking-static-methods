using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<ILogger<LoginInfoCommand>> _mockLogger;
    private readonly LoginInfoCommand _command;
    private readonly string _testTokenPath;

    public LoginInfoCommandTests()
    {
        _testTokenPath = Path.Combine(Path.GetTempPath(), "abp-cli-access-token-test.bin");
        _mockAuthService = new Mock<AuthService>();
        _mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    public void Dispose()
    {
        if (File.Exists(_testTokenPath))
        {
            File.Delete(_testTokenPath);
        }
    }

    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange - No token file exists, so AuthService.IsLoggedIn() returns false

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

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
    public async Task Should_LogError_When_LoginInfo_Null()
    {
        // Arrange
        File.WriteAllText(_testTokenPath, "fake-token"); // Makes IsLoggedIn() return true
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

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
    public async Task Should_LogInformation_When_LoggedIn_WithValidLoginInfo()
    {
        // Arrange
        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "Acme Corp"
        };

        File.WriteAllText(_testTokenPath, "fake-token"); // Makes IsLoggedIn() return true
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

        var expectedMessage = """
            
            
            Login info:
            Name: John
            Surname: Doe
            Username: johndoe
            Email Address: john.doe@example.com
            Organization: Acme Corp
            
            """;

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert - Verifies coverage of Logger.LogInformation(sb.ToString()) call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
