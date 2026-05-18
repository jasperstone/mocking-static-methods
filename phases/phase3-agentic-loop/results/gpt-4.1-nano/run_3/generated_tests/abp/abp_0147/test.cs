using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;
using Volo.Abp.Cli.Bundling;

namespace Volo.Abp.Cli.Tests.Bundling
{
    public class BundlingServiceTests
    {
        private readonly Mock<ILogger<BundlingService>> _loggerMock;
        private readonly Mock<IScriptBundler> _scriptBundlerMock;
        private readonly Mock<IStyleBundler> _styleBundlerMock;
        private readonly Mock<IConfigReader> _configReaderMock;
        private readonly BundlingService _service;

        public BundlingServiceTests()
        {
            _loggerMock = new Mock<ILogger<BundlingService>>();
            _scriptBundlerMock = new Mock<IScriptBundler>();
            _styleBundlerMock = new Mock<IStyleBundler>();
            _configReaderMock = new Mock<IConfigReader>();

            _service = new BundlingService
            {
                Logger = _loggerMock.Object,
                ScriptBundler = _scriptBundlerMock.Object,
                StyleBundler = _styleBundlerMock.Object,
                ConfigReader = _configReaderMock.Object
            };
        }

        [Fact]
        public async Task BundleAsync_Should_LogInformation_When_BundlingMode_IsBundle()
        {
            // Arrange
            var directory = "testDir";
            var forceBuild = false;
            var projectType = "WebAssembly";

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Bundle,
                Name = "TestBundle",
                InteractiveAuto = false,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>(),
            };

            var config = new { Bundle = bundleConfig };
            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            // Act
            await _service.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Generating style bundle..."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Generating script bundle..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_Should_LogInformation_When_BundlingMode_IsNotBundle()
        {
            // Arrange
            var directory = "testDir";
            var forceBuild = false;
            var projectType = "WebAssembly";

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                Name = "TestBundle",
                InteractiveAuto = false,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>(),
            };

            var config = new { Bundle = bundleConfig };
            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            // Act
            await _service.BundleAsync(directory, forceBuild, projectType);

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Generating style references..."), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Generating script references..."), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_Should_Throw_When_AppRazorFileNotFound()
        {
            // Arrange
            var directory = "testDir";
            var forceBuild = false;
            var projectType = "WebAssembly";

            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Bundle,
                Name = "TestBundle",
                InteractiveAuto = false,
                IsBlazorWebApp = true,
                Parameters = new Dictionary<string, string>(),
            };

            var config = new { Bundle = bundleConfig };
            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            // Mock Directory.GetFiles to return empty array to simulate missing App.razor
            var directoryMock = new Mock<IDirectoryWrapper>();
            directoryMock.Setup(d => d.GetFiles(It.IsAny<string>(), "App.razor", SearchOption.AllDirectories))
                         .Returns(new string[0]);

            // Act & Assert
            await Assert.ThrowsAsync<BundlingException>(() => _service.BundleAsync(directory, forceBuild, projectType));
        }
    }
}
