using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _mockLogger;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            var mockAppHost = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var appVersion = new Version(1, 0, 0, 0);

            _pluginManager = new PluginManager(_mockLogger.Object, mockAppHost.Object, config, pluginsPath, appVersion);
        }

        [Fact]
        public void SaveManifest_InvalidValue_LogsWarning()
        {
            // Arrange
            var manifest = new PluginManifest();
            var path = "invalid/path";

            // Act
            var result = _pluginManager.SaveManifest(manifest, path);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
