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
        private readonly string _pluginsPath;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _pluginsPath = Path.GetTempPath();
        }

        [Fact]
        public void SaveManifest_ArgumentException_LogsWarning()
        {
            // Arrange
            var invalidPath = "/invalid/{path";
            var manifest = new PluginManifest();
            var pluginManager = CreatePluginManager();

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(v => v.ToString().Contains("Unable to save plugin manifest due to invalid value. {Path}") && v.ToString().Contains(invalidPath)),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ValidPath_ReturnsTrue()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var path = tempDir;
            var manifest = new PluginManifest { Name = "TestPlugin" };
            var pluginManager = CreatePluginManager();

            try
            {
                // Act
                var result = pluginManager.SaveManifest(manifest, path);

                // Assert
                Assert.True(result);
                var metaFile = Path.Combine(path, "meta.json");
                Assert.True(File.Exists(metaFile));
                var content = File.ReadAllText(metaFile);
                Assert.Contains("\"Name\":\"TestPlugin\"", content);
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
            // Create minimal mocks/stubs for required dependencies
            var logger = _loggerMock.Object;
            var appHostMock = new Mock<IServerApplicationHost>();
            appHostMock.Setup(x => x.Resolve<IHttpClientFactory>()).Returns(Mock.Of<IHttpClientFactory>());
            var config = new ServerConfiguration();

            return new PluginManager(logger, appHostMock.Object, config, _pluginsPath, new Version(10, 8, 0));
        }
    }
}
