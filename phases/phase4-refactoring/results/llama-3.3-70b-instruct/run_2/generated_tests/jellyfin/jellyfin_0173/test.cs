using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public async Task SaveManifest_LogsWarning_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, null);
            var manifest = new PluginManifest { Name = "TestPlugin", Version = new Version(1, 0, 0, 0), Status = PluginStatus.Disabled };
            var path = Path.GetTempFileName();

            // Act and Assert
            var result = pluginManager.SaveManifest(manifest, path);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<ArgumentException>(), "Unable to save plugin manifest due to invalid value. {Path}", path), Times.Once);
            Assert.False(result);
        }

        [Fact]
        public async Task SaveManifest_ReturnsTrue_WhenNoExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, null);
            var manifest = new PluginManifest { Name = "TestPlugin", Version = new Version(1, 0, 0, 0), Status = PluginStatus.Disabled };
            var path = Path.GetTempFileName();

            // Act and Assert
            var result = pluginManager.SaveManifest(manifest, path);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<ArgumentException>(), "Unable to save plugin manifest due to invalid value. {Path}", path), Times.Never);
            Assert.True(result);
        }
    }
}
