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
        private readonly Mock<ILogger<BundlingService>> _loggerMock;
        private readonly Mock<IScriptBundler> _scriptBundlerMock;
        private readonly Mock<IStyleBundler> _styleBundlerMock;
        private readonly Mock<IConfigReader> _configReaderMock;
        private readonly BundlingService _bundlingService;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
            _scriptBundlerMock = new Mock<IScriptBundler>();
            _styleBundlerMock = new Mock<IStyleBundler>();
            _configReaderMock = new Mock<IConfigReader>();

            _bundlingService = new BundlingService
            {
                Logger = _loggerMock.Object,
                ScriptBundler = _scriptBundlerMock.Object,
                StyleBundler = _styleBundlerMock.Object,
                ConfigReader = _configReaderMock.Object
            };
        }

        [Fact]
        public async Task BundleAsync_ShouldLogScriptReferencesGeneration()
        {
            // Arrange
            var directory = "testDirectory";
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig { Mode = BundlingMode.None };
            var bundleDefinitions = new List<BundleTypeDefinition>();

            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

            // Act
            await _bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
