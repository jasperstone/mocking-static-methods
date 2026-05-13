using System;
using System.IO;
using System.Text.Json;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();

            // Create minimal dependencies for constructor
            var pluginsPath = Path.Combine(Path.GetTempPath(), "plugins");
            Directory.CreateDirectory(pluginsPath);
            
            _pluginManager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                pluginsPath,
                new Version(10, 8, 0));
        }

        [Fact]
        public void SaveManifest_ArgumentException_LogsWarning()
        {
            // Arrange
            var manifest = new PluginManifest();
            var invalidPath = string.Empty; // Invalid path that will cause ArgumentException in Path.Combine

            // Act
            var result = _pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<Microsoft.Extensions.Logging.FormattedLogValues>>(lv => 
                        lv.ToString().Contains("Unable to save plugin manifest due to invalid value") &&
                        lv.ToString().Contains(invalidPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<Microsoft.Extensions.Logging.FormattedLogValues, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_SuccessfulWrite_ReturnsTrue()
        {
            // Arrange
            var manifest = new PluginManifest { Name = "TestPlugin", Id = "test" };
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var path = tempDir;

            try
            {
                // Act
                var result = _pluginManager.SaveManifest(manifest, path);

                // Assert
                Assert.True(result);
                var metaFile = Path.Combine(path, "meta.json");
                Assert.True(File.Exists(metaFile));
                var content = File.ReadAllText(metaFile);
                Assert.Contains("TestPlugin", content);
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
