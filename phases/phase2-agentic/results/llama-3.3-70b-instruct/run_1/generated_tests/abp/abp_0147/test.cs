using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class BundlingServiceTests
    {
        private readonly Mock<ILogger<BundlingService>> _loggerMock;
        private readonly Mock<IScriptBundler> _scriptBundlerMock;
        private readonly Mock<IStyleBundler> _styleBundlerMock;
        private readonly BundlingService _bundlingService;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
            _scriptBundlerMock = new Mock<IScriptBundler>();
            _styleBundlerMock = new Mock<IStyleBundler>();
            _bundlingService = new BundlingService(_loggerMock.Object, _scriptBundlerMock.Object, _styleBundlerMock.Object);
        }

        [Fact]
        public async Task BundleAsync_StyleBundleGeneratedSuccessfully_LogsInformation()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig { Mode = BundlingMode.Bundle };
            var bundleDefinitions = new List<BundleTypeDefinition>();
            var styleContext = new BundleContext();
            var scriptContext = new BundleContext();
            var styleDefinitions = "style_definitions";
            var scriptDefinitions = "script_definitions";

            _scriptBundlerMock.Setup(sb => sb.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns(scriptDefinitions);
            _styleBundlerMock.Setup(sb => sb.Bundle(It.IsAny<BundleOptions>(), It.IsAny<BundleContext>())).Returns(styleDefinitions);

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Generating style bundle..."), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Style bundle has been generated successfully."), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Generating script bundle..."), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Script bundle has been generated successfully."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_StyleReferencesGeneratedSuccessfully_LogsInformation()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig { Mode = BundlingMode.Reference };
            var bundleDefinitions = new List<BundleTypeDefinition>();
            var styleContext = new BundleContext();
            var scriptContext = new BundleContext();
            var styleDefinitions = "style_definitions";
            var scriptDefinitions = "script_definitions";

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Generating style references..."), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Generating script references..."), Times.Once);
        }
    }
}
