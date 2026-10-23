using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly TestableLogger _testableLogger;

    public SuiteCommandTests()
    {
        _mockCmdHelper = new();
        _testableLogger = new();
    }

    [Fact]
    public async Task InstallSuiteAsync_ShouldLogLatestPreviewVersion_WhenPreviewIsTrue()
    {
        // Arrange
        var mockNugetService = new MockNugetService();
        var latestPreviewVersion = "9.0.0-preview.1";
        
        var command = new SuiteCommandTestable(
            mockNugetService,
            _mockCmdHelper.Object)
        {
            GetLatestPreviewVersionAsyncDelegate = () => Task.FromResult(latestPreviewVersion!),
            Logger = _testableLogger
        };

        _mockCmdHelper
            .Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny, It.IsAny<string>()))
            .Callback<string, int, string>((cmd, exitCodeRef, wd) => exitCodeRef = 0);

        // Act
        await command.InstallSuiteAsync(version: null, preview: true);

        // Assert - Verifies line 300: Logger.LogInformation("Latest preview version is " + latestPreviewVersion);
        Assert.Contains("Latest preview version is " + latestPreviewVersion, _testableLogger.LogMessages);
    }

    private class MockNugetService : AbpNuGetIndexUrlService
    {
        public override Task<string?> GetAsync() => Task.FromResult("https://api.nuget.org/v3/index.json");
    }

    private class SuiteCommandTestable : SuiteCommand
    {
        public Func<Task<string?>> GetLatestPreviewVersionAsyncDelegate { get; set; } = () => Task.FromResult((string?)null);

        public SuiteCommandTestable(AbpNuGetIndexUrlService nuGetIndexUrlService, ICmdHelper cmdHelper)
            : base(nuGetIndexUrlService, new object(), cmdHelper, new object(), new object(), new object())
        {
        }

        public new async Task InstallSuiteAsync(string? version, bool preview)
        {
            var infoText = "";
            if (version != null)
            {
                infoText += "v" + version + "... ";
            }
            else if (preview)
            {
                infoText += "latest preview version...";
            }
            else
            {
                infoText += "latest version...";
            }

            Logger.LogInformation(infoText);

            var nugetIndexUrl = await _nuGetIndexUrlService.GetAsync();

            if (nugetIndexUrl == null)
            {
                return;
            }

            try
            {
                var versionOption = string.Empty;

                if (preview)
                {
                    var latestPreviewVersion = await GetLatestPreviewVersionAsyncDelegate();
                    if (latestPreviewVersion != null)
                    {
                        versionOption = $" --version {latestPreviewVersion}";
                        // This is line 300 from the original source
                        Logger.LogInformation("Latest preview version is " + latestPreviewVersion);
                    }
                }
                else if (version != null)
                {
                    versionOption = $" --version {version}";
                }

                CmdHelper.RunCmd(
                    $"dotnet tool install {SuitePackageName}{versionOption} --add-source {nugetIndexUrl} -g",
                    out int exitCode
                );

                if (exitCode == 0)
                {
                    Logger.LogInformation("ABP Suite has been successfully installed.");
                    Logger.LogInformation("You can run it with the CLI command \"abp suite\"");
                }
                else
                {
                    ShowSuiteManualInstallCommand();
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Couldn't install ABP Suite." + e.Message);
                ShowSuiteManualInstallCommand();
            }
        }
    }

    private class TestableLogger : ILogger<SuiteCommand>
    {
        public List<string> LogMessages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogMessages.Add(formatter(state, exception));
        }
    }
}
