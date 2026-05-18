using Xunit;
using Moq;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Build;
using Volo.Abp.Cli.Version;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BundlingServiceTests
{
    [Fact]
    public async Task BundleAsync_ShouldLogInformation_WhenGeneratingScriptReferences()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BundlingService>>();
        var scriptBundlerMock = new Mock<IScriptBundler>();
        var styleBundlerMock = new Mock<IStyleBundler>();
        var configReaderMock = new Mock<IConfigReader>();
        var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
        var jsMinifierMock = new Mock<IJavascriptMinifier>();
        var cssMinifierMock = new Mock<ICssMinifier>();

        var bundlingService = new BundlingService
        {
            Logger = loggerMock.Object,
            ScriptBundler = scriptBundlerMock.Object,
            StyleBundler = styleBundlerMock.Object,
            ConfigReader = configReaderMock.Object,
            DotNetProjectBuilder = dotNetProjectBuilderMock.Object,
            JsMinifier = jsMinifierMock.Object,
            CssMinifier = cssMinifierMock.Object,
            CliVersionService = new CliVersionService()
        };

        var bundleConfig = new BundleConfig
        {
            Mode = BundlingMode.Reference,
            InteractiveAuto = false,
            IsBlazorWebApp = false
        };

        configReaderMock.Setup(x => x.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

        // Act
        await bundlingService.BundleAsync("testDirectory", false, "WebAssembly");

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Generating script references..."),
            Times.Once);
    }
}
