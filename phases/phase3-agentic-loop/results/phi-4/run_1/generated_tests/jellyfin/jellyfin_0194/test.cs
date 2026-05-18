using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Plugins;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private class TestPluginManager : PluginManager
        {
            public TestPluginManager(ILogger<PluginManager> logger, IServerApplicationHost appHost, ServerConfiguration config, string pluginsPath, Version appVersion)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
            }

            public new bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                return base.ChangePluginState(plugin, status);
            }
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenUnableToEnablePreviousVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new TestPluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                string.Empty,
                new Version(1, 0, 0));

            var previousVersion = new LocalPlugin
            {
                Id = "plugin-id",
                Version = "1.0.0",
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                },
                Name = "Test Plugin"
            };

            var currentPlugin = new LocalPlugin
            {
                Id = "plugin-id",
                Version = "2.0.0",
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                }
            };

            // Mock the ChangePluginState method to return false
            pluginManager.ChangePluginState = (plugin, status) => false;

            // Act
            pluginManager.ProcessAlternative(currentPlugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Unable to enable version {Version} of {Name}")),
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenUnableToSupersedePreviousVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new TestPluginManager(
                loggerMock.Object,
                null, // Mock or provide a suitable IServerApplicationHost
                null, // Mock or provide a suitable ServerConfiguration
                string.Empty,
                new Version(1, 0, 0));

            var previousVersion = new LocalPlugin
            {
                Id = "plugin-id",
                Version = "1.0.0",
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Superseded
                },
                Name = "Test Plugin"
            };

            var currentPlugin = new LocalPlugin
            {
                Id = "plugin-id",
                Version = "2.0.0",
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Superseded
                }
            };

            // Mock the ChangePluginState method to return false
            pluginManager.ChangePluginState = (plugin, status) => false;

            // Act
            pluginManager.ProcessAlternative(currentPlugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Unable to supercede version {Version} of {Name}")),
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }
    }
}
