using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Bundling;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class BundlingServiceTests
    {
        private readonly Mock<ILogger<BundlingService>> _loggerMock;
        private readonly Mock<IBundleContextFactory> _bundleContextFactoryMock;
        private readonly Mock<IScriptBundler> _scriptBundlerMock;
        private readonly Mock<IStyleBundler> _styleBundlerMock;
        private readonly BundlingService _bundlingService;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
            _bundleContextFactoryMock = new Mock<IBundleContextFactory>();
            _scriptBundlerMock = new Mock<IScriptBundler>();
            _styleBundlerMock = new Mock<IStyleBundler>();

            _bundlingService = new BundlingService(
                _loggerMock.Object,
                _bundleContextFactoryMock.Object,
                _scriptBundlerMock.Object,
                _styleBundlerMock.Object);
        }

        [Fact]
        public async Task BundleAsync_LogInformation_Called()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task BundleAsync_LogInformation_StyleBundle_Called()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Generating style bundle..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_LogInformation_ScriptBundle_Called()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Generating script bundle..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_LogInformation_StyleReferences_Called()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Generating style references..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_LogInformation_ScriptReferences_Called()
        {
            // Arrange
            var directory = "test_directory";
            var forceBuild = true;
            var projectType = BundlingConsts.WebAssembly;

            // Act
            await _bundlingService.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Generating script references..."), Times.Once);
        }
    }
}
