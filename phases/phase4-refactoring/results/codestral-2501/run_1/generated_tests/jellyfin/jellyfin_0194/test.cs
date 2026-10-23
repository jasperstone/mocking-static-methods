using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_WhenPreviousVersionIsNull_ShouldSetPluginStatusToRestart()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new Mock<PluginManager>(loggerMock.Object, null, null, null, null);
            var plugin = new LocalPlugin
            {
                Id = "testPlugin",
                Version = new System.Version(1, 0, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                }
            };

            // Act
            pluginManager.Object.ProcessAlternative(plugin);

            // Assert
            Assert.Equal(PluginStatus.Restart, plugin.Manifest.Status);
            Assert.False(plugin.Manifest.AutoUpdate);
        }

        [Fact]
        public void ProcessAlternative_WhenChangePluginStateFails_ShouldLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new Mock<PluginManager>(loggerMock.Object, null, null, null, null);
            var plugin = new LocalPlugin
            {
                Id = "testPlugin",
                Version = new System.Version(1, 0, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                }
            };
            var previousVersion = new LocalPlugin
            {
                Id = "testPlugin",
                Version = new System.Version(0, 9, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                }
            };

            pluginManager.Object._plugins = new List<LocalPlugin> { previousVersion };

            // Act
            pluginManager.Object.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "Unable to enable version {Version} of {Name}",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_WhenPluginStatusIsSupersededAndChangePluginStateFails_ShouldLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new Mock<PluginManager>(loggerMock.Object, null, null, null, null);
            var plugin = new LocalPlugin
            {
                Id = "testPlugin",
                Version = new System.Version(1, 0, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Superseded
                }
            };
            var previousVersion = new LocalPlugin
            {
                Id = "testPlugin",
                Version = new System.Version(0, 9, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                }
            };

            pluginManager.Object._plugins = new List<LocalPlugin> { previousVersion };

            // Act
            pluginManager.Object.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "Unable to supercede version {Version} of {Name}",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
