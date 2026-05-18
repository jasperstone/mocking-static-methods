using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NuGet.Versioning;
using System;
using System.IO;
using Xunit;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Memory;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Internal.Telemetry;

namespace Volo.Abp.Cli.Tests;

public class CliServiceTests
{
    private readonly Mock<ILogger<CliService>> _loggerMock;

    public CliServiceTests()
    {
        _loggerMock = new Mock<ILogger<CliService>>();
        _loggerMock.SetupAllProperties();
    }

    [Fact]
    public void LogNewVersionInfo_StableChannel_LogsCorrectMessages()
    {
        // Arrange
        var cliService = CreateTestableCliService();
        var toolPath = @"C:\some\path";

        // Act
        GetLogNewVersionInfoMethod(cliService)(UpdateChannel.Stable, new SemanticVersion(99, 0, 0), toolPath);

        // Assert
        _loggerMock.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("A newer stable version of the ABP CLI is available: 99.0.0.")), 
            It.IsAny<object[]>()), Times.Once);

        _loggerMock.Verify(l => l.LogWarning("dotnet tool update --tool-path C:\\some\\path Volo.Abp.Cli"), Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_PrereleaseChannel_LogsCorrectMessages()
    {
        // Arrange
        var cliService = CreateTestableCliService();
        var toolPath = @"C:\some\path";

        // Act
        GetLogNewVersionInfoMethod(cliService)(UpdateChannel.Prerelease, new SemanticVersion(99, 0, 0), toolPath);

        // Assert
        _loggerMock.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("A newer prerelease version")), 
            It.IsAny<object[]>()), Times.Once);

        _loggerMock.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("dotnet tool update --tool-path C:\\some\\path Volo.Abp.Cli --version 99.0.0"))), 
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_NightlyChannel_LogsCorrectMessages()
    {
        // Arrange
        var cliService = CreateTestableCliService();
        var toolPath = @"C:\some\path";

        // Act
        GetLogNewVersionInfoMethod(cliService)(UpdateChannel.Nightly, new SemanticVersion(99, 0, 0), toolPath);

        // Assert
        _loggerMock.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("A newer nightly version")), 
            It.IsAny<object[]>()), Times.Once);

        _loggerMock.Verify(l => l.LogWarning("dotnet tool uninstall --tool-path C:\\some\\path Volo.Abp.Cli"), Times.Once);
        _loggerMock.Verify(l => l.LogWarning(
            It.Is<string>(s => s.Contains("--add-source https://www.myget.org/F/abp-nightly"))), 
            Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_GlobalTool_UsesCorrectFlag()
    {
        // Arrange
        var cliService = CreateTestableCliService();
        var globalToolPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\.dotnet\tools\");

        // Act
        GetLogNewVersionInfoMethod(cliService)(UpdateChannel.Stable, new SemanticVersion(99, 0, 0), globalToolPath);

        // Assert
        _loggerMock.Verify(l => l.LogWarning("dotnet tool update -g Volo.Abp.Cli"), Times.Once);
    }

    [Fact]
    public void LogNewVersionInfo_WithCustomMessage_LogsAllMessages()
    {
        // Arrange
        var cliService = CreateTestableCliService();
        var toolPath = @"C:\some\path";
        var customMessage = "This is a custom message";

        // Act
        GetLogNewVersionInfoMethod(cliService)(UpdateChannel.Stable, new SemanticVersion(99, 0, 0), toolPath, customMessage);

        // Assert
        _loggerMock.Verify(l => l.LogWarning(customMessage), Times.Once);
    }

    private TestableCliService CreateTestableCliService()
    {
        return new TestableCliService(
            new Mock<ICommandLineArgumentParser>().Object,
            new Mock<ICommandSelector>().Object,
            new Mock<IServiceScopeFactory>().Object,
            new Mock<PackageVersionCheckerService>().Object,
            new Mock<ICmdHelper>().Object,
            new Mock<MemoryService>().Object,
            new Mock<CliVersionService>().Object,
            new Mock<ITelemetryService>().Object
        )
        {
            Logger = _loggerMock.Object
        };
    }

    private static Action<UpdateChannel, SemanticVersion, string, string> GetLogNewVersionInfoMethod(CliService service)
    {
        return (channel, version, path, message) => 
            typeof(CliService).GetMethod("LogNewVersionInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(service, new object?[] { channel, version, path, message })!;
    }

    public class TestableCliService : CliService
    {
        public TestableCliService(
            ICommandLineArgumentParser commandLineArgumentParser,
            ICommandSelector commandSelector,
            IServiceScopeFactory serviceScopeFactory,
            PackageVersionCheckerService nugetService,
            ICmdHelper cmdHelper,
            MemoryService memoryService,
            CliVersionService cliVersionService,
            ITelemetryService telemetryService)
            : base(commandLineArgumentParser, commandSelector, serviceScopeFactory, nugetService, cmdHelper, memoryService, cliVersionService, telemetryService)
        {
        }
    }
}
