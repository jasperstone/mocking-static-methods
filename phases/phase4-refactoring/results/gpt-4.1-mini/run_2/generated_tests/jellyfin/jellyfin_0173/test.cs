using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Model.Plugins
{
    // Minimal stub for PluginManifest to allow compilation
    internal class PluginManifest
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
    }
}

namespace Emby.Server.Implementations.Tests.Plugins
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

            var manifest = new MediaBrowser.Model.Plugins.PluginManifest
            {
                Id = "test",
                Name = "Test Plugin",
                Version = "1.0.0"
            };

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            // Act
            var result = pluginManager.SaveManifest(manifest, tempDir);

            // Assert
            Assert.True(result);
            var filePath = Path.Combine(tempDir, "meta.json");
            Assert.True(File.Exists(filePath));

            // Cleanup
            File.Delete(filePath);
            Directory.Delete(tempDir);
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

            var manifest = new MediaBrowser.Model.Plugins.PluginManifest
            {
                Id = "test",
                Name = "Test Plugin",
                Version = "1.0.0"
            };

            // Use an invalid path to cause ArgumentException on File.WriteAllText
            var invalidPath = "\0invalid_path";

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest")),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
