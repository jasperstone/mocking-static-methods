using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Minify.Scripts;
using Volo.Abp.Minify.Styles;
using Volo.Abp.Modularity;

namespace Volo.Abp.Cli.Tests.Bundling
{
    public class BundlingServiceTests
    {
        [Fact]
        public async Task BundleAsync_LogsGeneratingScriptReferences()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BundlingService>>();
            var scriptBundlerMock = new Mock<IScriptBundler>();
            var styleBundlerMock = new Mock<IStyleBundler>();
            var configReaderMock = new Mock<IConfigReader>();
            var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();
            var jsMinifierMock = new Mock<IJavascriptMinifier>();
            var cssMinifierMock = new Mock<ICssMinifier>();
            var cliVersionServiceMock = new Mock<CliVersionService>();

            var bundlingService = new BundlingService
            {
                Logger = loggerMock.Object,
                ScriptBundler = scriptBundlerMock.Object,
                StyleBundler = styleBundlerMock.Object,
                ConfigReader = configReaderMock.Object,
                DotNetProjectBuilder = dotNetProjectBuilderMock.Object,
                JsMinifier = jsMinifierMock.Object,
                CssMinifier = cssMinifierMock.Object,
                CliVersionService = cliVersionServiceMock.Object
            };

            var directory = "test_directory";
            var forceBuild = false;
            var projectType = BundlingConsts.WebAssembly;

            // Act
            await bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
