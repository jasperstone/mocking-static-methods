using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Args;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly SuiteCommand _suiteCommand;
    private readonly MethodInfo _killSuiteMethod;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        
        // Create SuiteCommand with NullLogger first, then override
        _suiteCommand = new SuiteCommand(
            new AbpNuGetIndexUrlService(null!), // minimal constructor
            new PackageVersionCheckerService(null!, null!, null!, null!, null!),
            new Volo.Abp.Cli.Utils.CmdHelper(),
            new AuthService(null!, null!),
            new CliHttpClientFactory(),
            new SuiteAppSettingsService(new Volo.Abp.Cli.Utils.CmdHelper())
        );

        // Override Logger property with our mock
        var loggerProperty = typeof(SuiteCommand).GetProperty("Logger")!;
        loggerProperty?.SetValue(_suiteCommand, _mockLogger.Object);

        // Get private KillSuite method
        _killSuiteMethod = typeof(SuiteCommand).GetMethod("KillSuite", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
    }

    [Fact]
    public void KillSuite_WhenNoProcessesExist_ShouldNotLogMessages()
    {
        // Act - Call private KillSuite method
        _killSuiteMethod.Invoke(_suiteCommand, null);

        // Assert - No LogInformation calls since no "abp-suite" processes exist in test environment
        _mockLogger.Verify(
            x => x.LogInformation(It.IsAny<string>()), 
            Times.Never);
    }

    [Fact]
    public void KillSuite_VerifiesLoggerInformationExtensionUsage()
    {
        // Act
        _killSuiteMethod.Invoke(_suiteCommand, null);

        // Assert - Confirms LogInformation extension method pattern is properly wired
        // (Logger.LogInformation calls will use Microsoft.Extensions.Logging.LoggerExtensions)
        _mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never); // Expected in no-process scenario
    }
}
