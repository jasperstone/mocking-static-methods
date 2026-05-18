using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    [Fact]
    public async Task ExecuteAsync_NotLoggedIn_ShouldLogErrorAndReturn()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(x => x.IsLoggedIn()).Returns(false);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };
        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        authServiceMock.Verify(x => x.IsLoggedIn(), Times.Once);
        loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You are not logged in.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        authServiceMock.Verify(x => x.GetLoginInfoAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_LoginInfoNull_ShouldLogErrorAndReturn()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync((LoginInfo)null);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };
        var commandLineArgs = new CommandLineArgs();

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get login info.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ValidLoginInfo_ShouldLogInformationWithCorrectFormat()
    {
        // Arrange
        var loginInfo = new LoginInfo
        {
            Name = "John",
            Surname = "Doe",
            Username = "johndoe",
            EmailAddress = "john.doe@example.com",
            Organization = "MyOrg"
        };

        var authServiceMock = new Mock<AuthService>();
        authServiceMock.Setup(x => x.IsLoggedIn()).Returns(true);
        authServiceMock.Setup(x => x.GetLoginInfoAsync()).ReturnsAsync(loginInfo);
        var loggerMock = new Mock<ILogger<LoginInfoCommand>>();
        var command = new LoginInfoCommand(authServiceMock.Object)
        {
            Logger = loggerMock.Object
        };
        var commandLineArgs = new CommandLineArgs();

        var expectedMessage = new StringBuilder();
        expectedMessage.AppendLine("");
        expectedMessage.AppendLine("Login info:");
        expectedMessage.AppendLine($"Name: {loginInfo.Name}");
        expectedMessage.AppendLine($"Surname: {loginInfo.Surname}");
        expectedMessage.AppendLine($"Username: {loginInfo.Username}");
        expectedMessage.AppendLine($"Email Address: {loginInfo.EmailAddress}");
        expectedMessage.AppendLine($"Organization: {loginInfo.Organization}");

        // Act
        await command.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            0,
            It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage.ToString()),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public void GetUsageInfo_ShouldReturnCorrectUsage()
    {
        // Arrange
        var authServiceMock = new Mock<AuthService>();
        var command = new LoginInfoCommand(authServiceMock.Object);

        // Act
        var result = command.GetUsageInfo();

        // Assert
        Assert.Contains("Usage:", result);
        Assert.Contains("abp login-info", result);
        Assert.Contains("https://abp.io/docs/latest/cli", result);
    }

    [Fact]
    public void GetShortDescription_ShouldReturnCorrectDescription()
    {
        // Arrange & Act
        var result = LoginInfoCommand.GetShortDescription();

        // Assert
        Assert.Equal("Show your login info.", result);
    }
}
