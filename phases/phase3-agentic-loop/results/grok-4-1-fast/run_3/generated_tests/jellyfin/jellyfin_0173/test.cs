using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly string _tempPath;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            _tempPath = Path.Combine(Path.GetTempPath(), "jellyfin-plugins-test-" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_tempPath);

            // Use NullLogger and minimal mocks to avoid missing type issues
            _pluginManager = new PluginManager(
                _loggerMock.Object,
                new Mock<IServerApplicationHost>().Object,
                new ServerConfiguration(),
                _tempPath,
                new Version(10, 8, 0));
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempPath))
            {
                try { Directory.Delete(_tempPath, true); } catch { }
            }
        }

        [Fact]
        public void SaveManifest_InvalidPath_LogsWarningAndReturnsFalse()
        {
            // Arrange
            var manifest = new PluginManifest();
            var invalidPath = string.Empty; // Causes ArgumentException in Path.Combine

            // Act
            var result = _pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v?.ToString().Contains("Unable to save plugin manifest") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ValidPath_ReturnsTrue()
        {
            // Arrange
            var manifest = new PluginManifest();
            var testPath = Path.Combine(_tempPath, "test-subdir");
            
            try
            {
                Directory.CreateDirectory(testPath);
                var expectedFile = Path.Combine(testPath, "meta.json");

                // Act
                var result = _pluginManager.SaveManifest(manifest, testPath);

                // Assert
                Assert.True(result);
                Assert.True(File.Exists(expectedFile));
                var content = File.ReadAllText(expectedFile);
                Assert.NotEmpty(content);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(testPath))
                        Directory.Delete(testPath, true);
                }
                catch { }
            }
        }
    }
}
