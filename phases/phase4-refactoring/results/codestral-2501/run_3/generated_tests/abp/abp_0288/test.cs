using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Utils;

public class SuiteCommandTests
{
    [Fact]
    public void KillSuite_LogsInformation_WhenSuiteProcessesAreKilled()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = mockLogger.Object
        };

        var mockProcess = new Mock<Process>();
        mockProcess.Setup(p => p.ProcessName).Returns("abp-suite");

        var suiteProcesses = new[]
        {
            mockProcess.Object,
            mockProcess.Object
        };

        var suiteCommandType = typeof(SuiteCommand);
        var getProcessesRelatedWithSuiteMethod = suiteCommandType.GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var killSuiteMethod = suiteCommandType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        getProcessesRelatedWithSuiteMethod.Invoke(suiteCommand, null);

        // Act
        killSuiteMethod.Invoke(suiteCommand, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suite closed.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(suiteProcesses.Length));
    }

    [Fact]
    public void KillSuite_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = mockLogger.Object
        };

        var suiteCommandType = typeof(SuiteCommand);
        var killSuiteMethod = suiteCommandType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        killSuiteMethod.Invoke(suiteCommand, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot close Suite.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
