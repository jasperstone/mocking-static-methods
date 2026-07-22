using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly Mock<AbpNuGetIndexUrlService> _mockNuGetIndexUrlService;
    private readonly Mock<PackageVersionCheckerService> _mockPackageVersionCheckerService;
    private readonly Mock<AuthService> _mockAuthService;
    private readonly Mock<CliHttpClientFactory> _mockCliHttpClientFactory;
    private readonly Mock<SuiteAppSettingsService> _mockSuiteAppSettingsService;
    private readonly Mock<ICmdHelper> _mockCmdHelper;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockLogger.SetupAllProperties();
        _mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>();
        _mockPackageVersionCheckerService = new Mock<PackageVersionCheckerService>();
        _mockAuthService = new Mock<AuthService>();
        _mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();
        _mockSuiteAppSettingsService = new Mock<SuiteAppSettingsService>();
        _mockCmdHelper = new Mock<ICmdHelper>();
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogLatestPreviewVersion_WhenPreviewIsTrueAndVersionFound()
    {
        // Arrange
        _mockNuGetIndexUrlService.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io");
        
        var suiteCommand = CreateSuiteCommand();
        var field = typeof(SuiteCommand).GetField("_latestPreviewVersion", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Set private field to trigger the log call at line 300
        field!.SetValue(suiteCommand, "8.0.0-preview.1");

        // Act
        await suiteCommand.InstallSuiteAsync(null, true);

        // Assert - verify Logger.LogInformation("Latest preview version is " + latestPreviewVersion)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => (v?.ToString() ?? "").Contains("Latest preview version is 8.0.0-preview.1")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldNotLogPreviewVersion_WhenPreviewIsTrueButNoVersionFound()
    {
        // Arrange
        _mockNuGetIndexUrlService.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io");
        
        var suiteCommand = CreateSuiteCommand();
        var field = typeof(SuiteCommand).GetField("_latestPreviewVersion", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Set private field to null (no version found)
        field!.SetValue(suiteCommand, (string?)null);

        // Act
        await suiteCommand.InstallSuiteAsync(null, true);

        // Assert - verify NO log call with "Latest preview version is"
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => (v?.ToString() ?? "").Contains("Latest preview version is")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        return new SuiteCommand(
            _mockNuGetIndexUrlService.Object,
            _mockPackageVersionCheckerService.Object,
            _mockCmdHelper.Object,
            _mockAuthService.Object,
            _mockCliHttpClientFactory.Object,
            _mockSuiteAppSettingsService.Object)
        {
            Logger = _mockLogger.Object
        };
    }
}
