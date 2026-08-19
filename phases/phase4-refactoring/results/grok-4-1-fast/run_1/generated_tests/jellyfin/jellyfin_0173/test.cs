using System;
using System.IO;
using System.Text.Json;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            Mock.Of<IServerApplicationHost>();
            new object(); // ServerConfiguration substitute
        }

        [Fact]
        public void SaveManifest_ArgumentException_LogsWarning()
        {
            // Arrange
            var pluginManager = CreatePluginManager();
            var manifest = new MediaBrowser.Common.Plugins.PluginManifest();

            // Act
            var result = pluginManager.SaveManifest(manifest, string.Empty);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ValidPath_ReturnsTrue()
        {
            // Arrange
            var pluginManager = CreatePluginManager();
            var manifest = new MediaBrowser.Common.Plugins.PluginManifest();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                var result = pluginManager.SaveManifest(manifest, tempDir);

                // Assert
                Assert.True(result);
                var filePath = Path.Combine(tempDir, "meta.json");
                Assert.True(File.Exists(filePath));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        private PluginManager CreatePluginManager()
        {
            var mockAppHost = Mock.Of<IServerApplicationHost>();
            var mockConfig = new object();
            var tempPluginsPath = Path.Combine(Path.GetTempPath(), "plugins", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPluginsPath);
            var appVersion = new Version(10, 8, 0);

            return new PluginManager(_loggerMock.Object, mockAppHost, mockConfig, tempPluginsPath, appVersion);
        }
    }
}
