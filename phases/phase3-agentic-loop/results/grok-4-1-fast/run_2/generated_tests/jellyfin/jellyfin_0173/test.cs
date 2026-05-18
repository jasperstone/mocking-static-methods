using System;
using System.IO;
using System.Text.Json;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<object> _appHostMock;
        private readonly object _configStub;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<object>();
            _configStub = new object();

            var tempPath = Path.Combine(Path.GetTempPath(), "jellyfin-plugins-test-" + Guid.NewGuid().ToString("N")[..8]);
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);

            _pluginManager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _configStub,
                tempPath,
                new Version(10, 8, 0));
        }

        [Fact]
        public void SaveManifest_ArgumentException_LogsWarning()
        {
            // Arrange
            var manifest = new { Name = "TestPlugin" }; // Simple anonymous object for serialization
            var invalidPath = string.Empty; // Causes ArgumentException in Path.Combine

            // Act
            var result = _pluginManager.SaveManifest((object)manifest, invalidPath);

            // Assert
            Assert.False(result);
            
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Unable to save plugin manifest due to invalid value. {Path}") &&
                        v.ToString()!.Contains(invalidPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_Success_ReturnsTrue()
        {
            // Arrange
            var manifest = new { Name = "TestPlugin", Version = "1.0.0.0" };
            var tempPath = Path.Combine(Path.GetTempPath(), "jellyfin-test-manifest-" + Guid.NewGuid().ToString("N")[..8]);
            
            try
            {
                Directory.CreateDirectory(tempPath);

                // Act
                var result = _pluginManager.SaveManifest((object)manifest, tempPath);

                // Assert
                Assert.True(result);
                var metaPath = Path.Combine(tempPath, "meta.json");
                Assert.True(File.Exists(metaPath));
                var content = File.ReadAllText(metaPath);
                Assert.Contains("TestPlugin", content);
            }
            finally
            {
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }
            }
        }
    }
}
