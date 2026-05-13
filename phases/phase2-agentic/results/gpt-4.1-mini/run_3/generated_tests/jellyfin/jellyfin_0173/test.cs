using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void SaveManifest_ValidManifest_WritesFileAndReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);
            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var manifest = new PluginManifest();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                var result = pluginManager.SaveManifest(manifest, tempDir);

                // Assert
                Assert.True(result);
                var filePath = Path.Combine(tempDir, "meta.json");
                Assert.True(File.Exists(filePath));
                var content = File.ReadAllText(filePath);
                Assert.Contains("\"Name\"", content); // PluginManifest has a Name property
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SaveManifest_InvalidPath_LogsWarningAndReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);
            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var manifest = new PluginManifest();

            // Use an invalid path to cause ArgumentException in Path.Combine or File.WriteAllText
            string invalidPath = "\0invalid_path";

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest due to invalid value.")),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
