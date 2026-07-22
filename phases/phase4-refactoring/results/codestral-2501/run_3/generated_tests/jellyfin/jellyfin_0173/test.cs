using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;
using System.Reflection;
using System;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _mockLogger;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            _pluginManager = new PluginManager(_mockLogger.Object, null, null, null, new Version(1, 0, 0, 0));
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
                    It.IsAny<ArgumentException>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);

            Assert.False(result);
        }
    }
}
