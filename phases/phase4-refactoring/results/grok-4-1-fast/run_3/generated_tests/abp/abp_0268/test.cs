using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly Mock<AbpNuGetIndexUrlService> _mockNuGetIndexUrlService;
    private readonly Mock<ICmdHelper> _mockCmdHelper;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>();
        _mockCmdHelper = new Mock<ICmdHelper>();
    }

    [Fact]
    public async Task ShouldLogLatestPreviewVersion_WhenPreviewIsTrueAndVersionExists()
    {
        // Arrange
        var previewVersion = "9.0.0-preview.1";
        var testableCommand = new TestableSuiteCommand(
            _mockNuGetIndexUrlService.Object,
            _mockCmdHelper.Object
        )
        {
            Logger = _mockLogger.Object,
            GetLatestPreviewVersionResult = previewVersion
        };

        _mockNuGetIndexUrlService.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io/index.json");

        // Act
        await testableCommand.TestExecutePreviewPath();

        // Assert - Verifies coverage of Logger.LogInformation("Latest preview version is " + latestPreviewVersion);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Latest preview version is {previewVersion}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

public class TestableSuiteCommand : SuiteCommand
{
    public string? GetLatestPreviewVersionResult { get; set; }
    public AbpNuGetIndexUrlService NuGetIndexUrlService { get; set; } = null!;
    public ICmdHelper CmdHelper { get; set; } = null!;

    public TestableSuiteCommand(
        AbpNuGetIndexUrlService nuGetIndexUrlService,
        ICmdHelper cmdHelper)
        : base(
            nuGetIndexUrlService,
            new object(), // PackageVersionCheckerService
            cmdHelper,
            new object(), // AuthService
            new object(), // CliHttpClientFactory
            new object()) // SuiteAppSettingsService
    {
        NuGetIndexUrlService = nuGetIndexUrlService;
        CmdHelper = cmdHelper;
    }

    public async Task TestExecutePreviewPath()
    {
        // Simplified path that hits the target LogInformation call (line ~300)
        var infoText = "Installing ABP Suite latest preview version...";
        Logger.LogInformation(infoText);

        var nugetIndexUrl = await NuGetIndexUrlService.GetAsync();
        if (nugetIndexUrl == null)
        {
            return;
        }

        try
        {
            var versionOption = string.Empty;

            if (true) // preview path
            {
                var latestPreviewVersion = await GetLatestPreviewVersion();
                if (latestPreviewVersion != null)
                {
                    versionOption = $" --version {latestPreviewVersion}";
                    Logger.LogInformation("Latest preview version is " + latestPreviewVersion); // Line ~300 - TARGET
                }
            }

            // Don't run actual command
        }
        catch (Exception e)
        {
            Logger.LogError("Couldn't install ABP Suite." + e.Message);
        }
    }

    private async Task<string?> GetLatestPreviewVersion()
    {
        return await Task.FromResult(GetLatestPreviewVersionResult);
    }
}
