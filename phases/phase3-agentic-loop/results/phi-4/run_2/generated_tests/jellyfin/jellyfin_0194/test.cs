using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MediaBrowser.Model.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFailsForActivePlugin()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PluginManager>>();
            var mockChangePluginState = new Mock<Func<LocalPlugin, PluginStatus, bool>>();
            mockChangePluginState.Setup(m => m(It.IsAny<LocalPlugin>(), PluginStatus.Superseded)).Returns(false);

            var pluginManager = new PluginManager(
                mockLogger.Object,
                null, // Mock or replace with a suitable IServerApplicationHost
                null, // Mock or replace with a suitable ServerConfiguration
                string.Empty,
                new Version(1, 0, 0)
            );

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            var changePluginStateField = typeof(PluginManager).GetField("_changePluginState", BindingFlags.NonPublic | BindingFlags.Instance);

            pluginsField.SetValue(pluginManager, new List<LocalPlugin>
            {
                new LocalPlugin
                {
                    Id = "plugin1",
                    Version = new Version(1, 0, 0),
                    Manifest = new PluginManifest { Status = PluginStatus.Active }
                },
                new LocalPlugin
                {
                    Id = "plugin1",
                    Version = new Version(0, 9, 9),
                    Manifest = new PluginManifest { Status = PluginStatus.Active }
                }
            });

            changePluginStateField.SetValue(pluginManager, mockChangePluginState.Object);

            // Act
            pluginManager.ProcessAlternative(pluginManager.Plugins.First(p => p.Version == new Version(1, 0, 0)));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Unable to enable version")),
                    It.IsAny<Version>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFailsForSupersededPlugin()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PluginManager>>();
            var mockChangePluginState = new Mock<Func<LocalPlugin, PluginStatus, bool>>();
            mockChangePluginState.Setup(m => m(It.IsAny<LocalPlugin>(), PluginStatus.Active)).Returns(false);

            var pluginManager = new PluginManager(
                mockLogger.Object,
                null, // Mock or replace with a suitable IServerApplicationHost
                null, // Mock or replace with a suitable ServerConfiguration
                string.Empty,
                new Version(1, 0, 0)
            );

            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            var changePluginStateField = typeof(PluginManager).GetField("_changePluginState", BindingFlags.NonPublic | BindingFlags.Instance);

            pluginsField.SetValue(pluginManager, new List<LocalPlugin>
            {
                new LocalPlugin
                {
                    Id = "plugin1",
                    Version = new Version(1, 0, 0),
                    Manifest = new PluginManifest { Status = PluginStatus.Superseded }
                },
                new LocalPlugin
                {
                    Id = "plugin1",
                    Version = new Version(0, 9, 9),
                    Manifest = new PluginManifest { Status = PluginStatus.Superseded }
                }
            });

            changePluginStateField.SetValue(pluginManager, mockChangePluginState.Object);

            // Act
            pluginManager.ProcessAlternative(pluginManager.Plugins.First(p => p.Version == new Version(1, 0, 0)));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Unable to supercede version")),
                    It.IsAny<Version>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}
