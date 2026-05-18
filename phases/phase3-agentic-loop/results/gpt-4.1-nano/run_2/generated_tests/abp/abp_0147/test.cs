using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Bundling;

namespace Volo.Abp.Cli.Tests.Bundling
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
        public async Task BundleAsync_Should_LogInformation_When_ModeIsBundleOrMinify()
        {
            // Arrange
            var directory = Path.GetTempPath();
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.Bundle,
                Name = "TestBundle",
                InteractiveAuto = false,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>(),
            };
            var config = new BundleConfiguration { Bundle = bundleConfig };
            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            // Act
            await _bundlingService.BundleAsync(directory, false, BundlingConsts.WebAssembly);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style bundle...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script bundle...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_Should_LogReferences_When_ModeIsNotBundle()
        {
            // Arrange
            var directory = Path.GetTempPath();
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                Name = "TestBundle",
                InteractiveAuto = false,
                IsBlazorWebApp = false,
                Parameters = new Dictionary<string, string>(),
            };
            var config = new BundleConfiguration { Bundle = bundleConfig };
            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            // Act
            await _bundlingService.BundleAsync(directory, false, BundlingConsts.WebAssembly);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating style references...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating script references...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public async Task BundleAsync_Should_Throw_When_AppRazorFileNotFound()
        {
            // Arrange
            var directory = Path.GetTempPath();
            var bundleConfig = new BundleConfig
            {
                Mode = BundlingMode.None,
                Name = "TestBundle",
                InteractiveAuto = false,
                IsBlazorWebApp = true,
                Parameters = new Dictionary<string, string>(),
            };
            var config = new BundleConfiguration { Bundle = bundleConfig };
            _configReaderMock.Setup(c => c.Read(It.IsAny<string>())).Returns(config);

            // Mock Directory.GetFiles to return empty array
            var directoryMock = new Mock<IDirectoryWrapper>();
            directoryMock.Setup(d => d.GetFiles(It.IsAny<string>(), "App.razor", SearchOption.AllDirectories))
                         .Returns(Array.Empty<string>());

            // Act & Assert
            await Assert.ThrowsAsync<BundlingException>(() => _bundlingService.BundleAsync(directory, false, BundlingConsts.WebAssembly));
        }
    }
}
