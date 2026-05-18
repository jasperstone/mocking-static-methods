using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Configuration;
using Xunit;

namespace Volo.Abp.Cli.Bundling.Tests
{
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
            var dotNetProjectBuilderMock = new Mock<IDotNetProjectBuilder>();

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                Name = null,
                InteractiveAuto = true,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>()
            };

            var config = new CliConfig
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
                DotNetProjectBuilder = dotNetProjectBuilderMock.Object
            };

            // Setup StyleBundler and ScriptBundler to return dummy strings
            styleBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("style bundle");
            scriptBundlerMock.Setup(s => s.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns("script bundle");

            // Setup Directory.GetFiles to return a dummy project file
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var csprojPath = Path.Combine(tempDir, "TestProject.csproj");
            File.WriteAllText(csprojPath, "<Project></Project>");

            // Act
            await bundlingService.BundleAsync(tempDir, forceBuild: false, projectType: "webassembly");

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

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
