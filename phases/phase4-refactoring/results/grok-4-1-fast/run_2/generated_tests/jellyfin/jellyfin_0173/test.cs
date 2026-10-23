using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
        }

        [Fact]
        public void SaveManifest_ArgumentException_LogsWarning()
        {
            // Arrange
            var pluginManager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                Path.GetTempPath(),
                new Version(10, 8, 0));

            var manifest = new PluginManifest();
            var invalidPath = string.Empty; // Will cause ArgumentException in Path.Combine

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<ArgumentException>(),
                    "Unable to save plugin manifest due to invalid value. {Path}",
                    invalidPath),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ValidPath_ReturnsTrue()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                var pluginManager = new PluginManager(
                    _loggerMock.Object,
                    _appHostMock.Object,
                    _config,
                    Path.GetTempPath(),
                    new Version(10, 8, 0));

                var manifest = new PluginManifest();

                // Act
                var result = pluginManager.SaveManifest(manifest, tempDir);

                // Assert
                Assert.True(result);
                var metaFile = Path.Combine(tempDir, "meta.json");
                Assert.True(File.Exists(metaFile));
                Assert.NotEmpty(File.ReadAllText(metaFile));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
