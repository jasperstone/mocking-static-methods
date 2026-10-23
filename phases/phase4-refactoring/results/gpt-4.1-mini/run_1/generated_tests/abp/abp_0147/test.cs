using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Bundling;

public class BundlingServiceTests
{
    [Fact]
    public async Task BundleAsync_LogsInformation_WhenGeneratingScriptReferences()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var styleBundlerMock = new Mock<IStyleBundler>();
        var scriptBundlerMock = new Mock<IScriptBundler>();
        var configReaderMock = new Mock<IConfigReader>();

        var bundleConfig = new BundleConfig
        {
            Mode = BundlingMode.None,
            InteractiveAuto = true,
            Name = "test",
            Parameters = new BundleParameterDictionary(),
            IsBlazorWebApp = false
        };

        var config = new AbpCliConfig
        {
            Bundle = bundleConfig
        };

        configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

        var bundlingService = new BundlingService
        {
            Logger = loggerMock.Object,
            StyleBundler = styleBundlerMock.Object,
            ScriptBundler = scriptBundlerMock.Object,
            ConfigReader = configReaderMock.Object,
            DotNetProjectBuilder = null,
            JsMinifier = null,
            CssMinifier = null,
            CliVersionService = null
        };

        // Act
        // We call the real BundleAsync but with a directory that has no csproj to short-circuit early
        // So we simulate the else branch by setting Mode to None and InteractiveAuto true
        // We expect the logger to be called with "Generating style references..." and "Generating script references..."
        // We will catch the exception thrown due to no csproj file to avoid test failure
        try
        {
            await bundlingService.BundleAsync("emptyDir", false, "webassembly");
        }
        catch (BundlingException)
        {
            // Expected due to no csproj file in test directory
        }

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style references...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script references...")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
