using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace Emby.Server.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void SaveManifest_InvalidValue_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));
            var manifest = new MediaBrowser.Model.Plugins.PluginManifest { Name = "Test Plugin" };
            var path = Path.GetTempFileName();

            // Act
            var result = pluginManager.SaveManifest(manifest, path);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<ArgumentException>(), "Unable to save plugin manifest due to invalid value. {Path}", path), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public void SaveManifest_ValidValue_SavesManifest()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));
            var manifest = new MediaBrowser.Model.Plugins.PluginManifest { Name = "Test Plugin" };
            var path = Path.GetTempFileName();

            // Act
            var result = pluginManager.SaveManifest(manifest, path);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(Path.Combine(path, PluginManager.MetafileName)));
        }
    }
}
