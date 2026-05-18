using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using System.IO;
using System.Text;

namespace Volo.Abp.Cli.Commands.Tests;

public class LoginInfoCommandTests
{
    private readonly Mock<ILogger<LoginInfoCommand>> _mockLogger;
    private readonly LoginInfoCommand _command;

    public LoginInfoCommandTests()
    {
        _mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        _mockLogger.SetupAllProperties();
        _command = new LoginInfoCommand(new FakeAuthService())
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogInformation_When_LoggedIn_And_GetValidLoginInfo()
    {
        // Arrange
        var tempTokenFile = Path.Combine(Path.GetTempPath(), "abp-cli-temp-token.txt");
        try
        {
            File.WriteAllText(tempTokenFile, "fake-token");

            var commandLineArgs = new CommandLineArgs();

            // Act
            await _command.ExecuteAsync(commandLineArgs);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Login info:") &&
                                                 v.ToString()!.Contains("Name:") &&
                                                 v.ToString()!.Contains("Organization:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }
        finally
        {
            if (File.Exists(tempTokenFile))
            {
                File.Delete(tempTokenFile);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        var commandLineArgs = new CommandLineArgs();

        // Act
        await _command.ExecuteAsync(commandLineArgs);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<string>(v => v == "You are not logged in."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_When_LoginInfoIsNull()
    {
        // Arrange
        var tempTokenFile = Path.Combine(Path.GetTempPath(), "abp-cli-temp-null.txt");
        var fakeAuthService = new FakeNullLoginInfoAuthService();
        _command.AuthService = fakeAuthService;
        
        try
        {
            File.WriteAllText(tempTokenFile, "fake-token");
            var commandLineArgs = new CommandLineArgs();

            // Act
            await _command.ExecuteAsync(commandLineArgs);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<string>(v => v == "Unable to get login info."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once
            );
        }
        finally
        {
            if (File.Exists(tempTokenFile))
            {
                File.Delete(tempTokenFile);
            }
            _command.AuthService = new FakeAuthService(); // Reset
        }
    }

    private class FakeAuthService : AuthService
    {
        public FakeAuthService() : base(
            Mock.Of<IIdentityModelAuthenticationService>(),
            NullLogger<AuthService>.Instance,
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<RemoteServiceExceptionHandler>(),
            Mock.Of<IJsonSerializer>())
        {
        }
    }

    private class FakeNullLoginInfoAuthService : AuthService
    {
        public FakeNullLoginInfoAuthService() : base(
            Mock.Of<IIdentityModelAuthenticationService>(),
            NullLogger<AuthService>.Instance,
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<RemoteServiceExceptionHandler>(),
            Mock.Of<IJsonSerializer>())
        {
        }

        public override async Task<LoginInfo> GetLoginInfoAsync()
        {
            return null;
        }
    }
}
