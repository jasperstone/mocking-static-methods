using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        [Fact]
        public void SaveManifest_LogsWarning_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, null);
            var manifest = new PluginInfo("Test", new Version(1, 0), "Test", Guid.NewGuid(), true);
            var path = string.Empty;

            // Act and Assert
            pluginManager.SaveManifest(manifest, path);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<ArgumentException>(), "Unable to save plugin manifest due to invalid value. {Path}", path), Times.Once);
        }

        [Fact]
        public void SaveManifest_ReturnsFalse_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, null);
            var manifest = new PluginInfo("Test", new Version(1, 0), "Test", Guid.NewGuid(), true);
            var path = string.Empty;

            // Act
            var result = pluginManager.SaveManifest(manifest, path);

            // Assert
            Assert.False(result);
        }
    }
}
