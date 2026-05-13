using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void SaveManifest_LogsWarning_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(
                loggerMock.Object,
                null, // Mock or replace with a suitable IServerApplicationHost
                null, // Mock or replace with a suitable ServerConfiguration
                string.Empty,
                new Version(1, 0, 0));

            var manifest = new PluginManifest();
            var path = "invalid/path";

            // Act
            var result = pluginManager.SaveManifest(manifest, path);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<ArgumentException>(),
                    "Unable to save plugin manifest due to invalid value. {Path}",
                    path),
                Times.Once);

            Assert.False(result);
        }
    }
}
