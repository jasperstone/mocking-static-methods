using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _mockLogger;
        private readonly Mock<IServerApplicationHost> _mockAppHost;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            _mockAppHost = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            _appVersion = new Version(1, 0, 0, 0);
        }

        [Fact]
        public void SaveManifest_InvalidValue_LogsWarning()
        {
            // Arrange
            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);
            var manifest = new PluginManifest();
            var path = "invalid/path";

            // Act
            var result = pluginManager.SaveManifest(manifest, path);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<ArgumentException>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
