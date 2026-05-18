using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Args;
using System.IO;
using System;

namespace Volo.Abp.Cli.Tests.Commands;

public class LoginInfo
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public bool HasSourceCodeAccess { get; set; }
}

public class LoginInfoCommandTests
{
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<ILogger<LoginInfoCommand>> _mockLogger;
    private readonly LoginInfoCommand _command;
    private readonly string _testAbpPath;
    private readonly string _testTokenPath;

    public LoginInfoCommandTests()
    {
        _mockAuthService = new Mock<AuthService>();
        _mockLogger = new Mock<ILogger<LoginInfoCommand>>();
        
        _command = new LoginInfoCommand(_mockAuthService.Object);
        _command.Logger = _mockLogger.Object;

        _testAbpPath = Path.Combine(Path.GetTempPath(), "abp-cli-test");
        Directory.CreateDirectory(Path.Combine(_testAbpPath, "cli"));
        _testTokenPath = Path.Combine(_testAbpPath, "cli", "access-token.bin");
    }

    private void SetupLoggedInState(bool createToken = true)
    {
        if (createToken)
        {
            File.WriteAllText(_testTokenPath, "test-token");
        }
        
        // Override CliPaths.AbpRootPath using reflection
        var field = typeof(CliPaths).GetField("AbpRootPath", 
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Public);
        field?.SetValue(null, _testAbpPath);
    }

    private void Cleanup()
    {
        try
        {
            if (File.Exists(_testTokenPath))
            {
                File.Delete(_testTokenPath);
            }
            if (Directory.Exists(_testAbpPath))
            {
                Directory.Delete(_testAbpPath, true);
            }
        }
        catch { }
    }

    [Fact]
    public async Task Should_LogError_When_NotLoggedIn()
    {
        // Arrange
        SetupLoggedInState(createToken: false);

        try
        {
            await _command.ExecuteAsync(new CommandLineArgs());

            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You are not logged in.")), 
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public async Task Should_LogError_When_LoginInfo_Null()
    {
        // Arrange
        SetupLoggedInState(createToken: true);
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).Returns((Task<LoginInfo>)(Task.FromResult<LoginInfo>(null!)));

        try
        {
            await _command.ExecuteAsync(new CommandLineArgs());

            _mockAuthService.Verify(x => x.GetLoginInfoAsync(), Times.Once);
            _mockLogger.Verify(
                x => x.Log(LogLevel.Error, It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to get login info.")), 
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            Cleanup();
        }
    }

    [Fact]
    public async Task Should_LogInformation_With_LoginInfo_When_Successful()
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

        SetupLoggedInState(createToken: true);
        _mockAuthService.Setup(x => x.GetLoginInfoAsync()).Returns((Task<LoginInfo>)(Task.FromResult(loginInfo)));

        var sb = new StringBuilder();
        sb.AppendLine("");
        sb.AppendLine("Login info:");
        sb.AppendLine($"Name: {loginInfo.Name}");
        sb.AppendLine($"Surname: {loginInfo.Surname}");
        sb.AppendLine($"Username: {loginInfo.Username}");
        sb.AppendLine($"Email Address: {loginInfo.EmailAddress}");
        sb.AppendLine($"Organization: {loginInfo.Organization}");
        var expectedMessage = sb.ToString();

        try
        {
            // Act
            await _command.ExecuteAsync(new CommandLineArgs());

            // Assert
            _mockAuthService.Verify(x => x.GetLoginInfoAsync(), Times.Once);
            _mockLogger.Verify(
                x => x.Log(LogLevel.Information, It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage), 
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            Cleanup();
        }
    }
}
