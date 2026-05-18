using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = "/plugins";
            _appVersion = new Version(10, 8, 0);
        }

        [Fact]
        public void SaveManifest_ArgumentException_LogsWarning()
        {
            // Arrange
            var pluginManager = CreatePluginManager();
            var manifest = new { Name = "TestPlugin" }; // Simple anonymous object that can be serialized
            var invalidPath = string.Empty; // Causes ArgumentException in Path.Combine

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(logValues => 
                        logValues.ToString().Contains("Unable to save plugin manifest due to invalid value. {Path}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ValidPath_ReturnsTrue()
        {
            // Arrange
            var pluginManager = CreatePluginManager();
            var manifest = new { Name = "TestPlugin" };
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                var result = pluginManager.SaveManifest(manifest, tempDir);

                // Assert
                Assert.True(result);
                var metaFile = Path.Combine(tempDir, "meta.json");
                Assert.True(File.Exists(metaFile));
                var content = File.ReadAllText(metaFile);
                Assert.NotEmpty(content);
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
            return new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                _pluginsPath,
                _appVersion);
        }
    }
}
