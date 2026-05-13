using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Bundling;
using Volo.Abp.Cli.Bundling;
using Volo.Abp.Cli.Bundling.Scripts;
using Volo.Abp.Cli.Bundling.Styles;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Minify.Scripts;
using Volo.Abp.Minify.Styles;
using Volo.Abp.Modularity;
using Xunit;

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
        public async Task BundleAsync_ShouldLogInformation_WhenGeneratingStyleBundle()
        {
            // Arrange
            var directory = "testDirectory";
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.BundleAndMinify
            };

            _configReaderMock.Setup(x => x.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

            // Act
            await _bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Generating style bundle..."),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation("Style bundle has been generated successfully."),
                Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ShouldLogInformation_WhenGeneratingScriptBundle()
        {
            // Arrange
            var directory = "testDirectory";
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.BundleAndMinify
            };

            _configReaderMock.Setup(x => x.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

            // Act
            await _bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Generating script bundle..."),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation("Script bundle has been generated successfully."),
                Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ShouldLogInformation_WhenGeneratingStyleReferences()
        {
            // Arrange
            var directory = "testDirectory";
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Reference
            };

            _configReaderMock.Setup(x => x.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

            // Act
            await _bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Generating style references..."),
                Times.Once);
        }

        [Fact]
        public async Task BundleAsync_ShouldLogInformation_WhenGeneratingScriptReferences()
        {
            // Arrange
            var directory = "testDirectory";
            var projectType = BundlingConsts.WebAssembly;
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Reference
            };

            _configReaderMock.Setup(x => x.Read(It.IsAny<string>())).Returns(new CliConfig { Bundle = bundleConfig });

            // Act
            await _bundlingService.BundleAsync(directory, false, projectType);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Generating script references..."),
                Times.Once);
        }
    }
}
