using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    public LoginInfoCommandTests()
    {
        _mockAuthService = new Mock<AuthService>();
        _mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
    {
        // Arrange - AuthService.IsLoggedIn() is static, so create token file to make it return true, then override
        File.WriteAllText(Volo.Abp.Cli.CliPaths.AccessToken, "fake-token");
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
    public async Task ExecuteAsync_Should_LogInformation_With_LoginInfo_When_Successful()
    {
        // Arrange
        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john@example.com",
            Organization = "Acme Corp"
        };

        File.WriteAllText(Volo.Abp.Cli.CliPaths.AccessToken, "fake-token");
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);

        var expectedMessage = new StringBuilder()
            .AppendLine("")
            .AppendLine("Login info:")
            .AppendLine("Name: John")
            .AppendLine("Surname: Doe")
            .AppendLine("Username: johndoe")
            .AppendLine("Email Address: john@example.com")
            .AppendLine("Organization: Acme Corp")
            .ToString();

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert - Verifies coverage of Logger.LogInformation(sb.ToString()) on line 49
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetUsageInfo_Should_Return_Expected_Usage()
    {
        // Act
        var result = _command.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", result);
        Assert.Contains("abp login-info", result);
        Assert.Contains("https://abp.io/docs/latest/cli", result);
    }

    [Fact]
    public void GetShortDescription_Should_Return_Description()
    {
        // Act
        var result = LoginInfoCommand.GetShortDescription();

        // Assert
        Assert.Equal("Show your login info.", result);
    }

    public void Dispose()
    {
        if (File.Exists(Volo.Abp.Cli.CliPaths.AccessToken))
        {
            File.Delete(Volo.Abp.Cli.CliPaths.AccessToken);
        }
    }
}
