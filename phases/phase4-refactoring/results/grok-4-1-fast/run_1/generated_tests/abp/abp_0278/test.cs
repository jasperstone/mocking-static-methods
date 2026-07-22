using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly SuiteCommand _suiteCommand;
    private readonly MethodInfo _showSuiteManualUpdateMethod;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _suiteCommand = new SuiteCommand(
            nuGetIndexUrlService: Mock.Of<AbpNuGetIndexUrlService>(),
            packageVersionCheckerService: Mock.Of<PackageVersionCheckerService>(),
            cmdHelper: Mock.Of<ICmdHelper>(),
            authService: Mock.Of<AuthService>(),
            cliHttpClientFactory: Mock.Of<CliHttpClientFactory>(),
            suiteAppSettingsService: Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = _mockLogger.Object
        };

        _showSuiteManualUpdateMethod = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_LogError_Line410()
    {
        // Act - Tests the LogError call specifically on line 410
        _showSuiteManualUpdateMethod.Invoke(_suiteCommand, null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString() == "You can also run the following command to update ABP Suite."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ShowSuiteManualUpdateCommand_Should_LogError_SecondCall()
    {
        // Act
        _showSuiteManualUpdateMethod.Invoke(_suiteCommand, null);

        // Assert - Tests the second LogError call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
