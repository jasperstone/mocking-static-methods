using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Minify.Scripts;
using Volo.Abp.Minify.Styles;
using Volo.Abp.Modularity;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Volo.Abp.Cli.Bundling.Tests
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_ShouldLogScriptReferencesGeneration()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var scriptBundlerMock = new Mock<IScriptBundler>();
            var styleBundlerMock = new Mock<IStyleBundler>();
            var configReaderMock = new Mock<IConfigReader>();
            var cliVersionServiceMock = new Mock<CliVersionService>();
            var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
            var jsMinifierMock = new Mock<IJavascriptMinifier>();
            var cssMinifierMock = new Mock<ICssMinifier>();

            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                ScriptBundler = scriptBundlerMock.Object,
                StyleBundler = styleBundlerMock.Object,
                ConfigReader = configReaderMock.Object,
                CliVersionService = cliVersionServiceMock.Object,
                DotNetProjectBuilder = dotNetProjectBuilderMock.Object,
                JsMinifier = jsMinifierMock.Object,
                CssMinifier = cssMinifierMock.Object
            };

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Reference,
                InteractiveAuto = false,
                IsBlazorWebApp = false
            };

            configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

            var directory = "testDirectory";
            var projectType = BundlingConstsWrapper.WebAssembly;

            // Act
            await bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
