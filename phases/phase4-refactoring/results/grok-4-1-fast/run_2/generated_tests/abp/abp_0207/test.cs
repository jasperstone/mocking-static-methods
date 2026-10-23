using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
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
        _command = new LoginInfoCommand(_mockAuthService.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_When_LoginInfo_Null()
    {
        // Arrange
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);

        // Act
        await _command.ExecuteAsync(new CommandLineArgs());

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Unable to get login info.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
                It.Is<It.IsAnyType>((v, t) => ((string)v) == expectedMessage),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
    public void GetShortDescription_Should_Return_Expected_Description()
    {
        // Act
        var result = LoginInfoCommand.GetShortDescription();

        // Assert
        Assert.Equal("Show your login info.", result);
    }
}
