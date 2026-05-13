using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly List<LocalPlugin> _plugins;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _plugins = new List<LocalPlugin>();
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenUnableToEnablePreviousVersion()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, null, null, null, null);
            var plugin = new LocalPlugin { Id = "TestPlugin", Version = "1.0", IsEnabledAndSupported = true, Manifest = new PluginManifest { Status = PluginStatus.Active } };
            var previousVersion = new LocalPlugin { Id = "TestPlugin", Version = "0.9", IsEnabledAndSupported = true, Name = "Test Plugin" };
            _plugins.Add(plugin);
            _plugins.Add(previousVersion);

            // Mock the ChangePluginState method to return false
            var pluginManagerMock = new Mock<PluginManager>(null, null, null, null, null);
            pluginManagerMock.Setup(pm => pm.ChangePluginState(It.IsAny<LocalPlugin>(), It.IsAny<PluginStatus>())).Returns(false);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Unable to enable version {Version} of {Name}")),
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenUnableToSupersedePreviousVersion()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, null, null, null, null);
            var plugin = new LocalPlugin { Id = "TestPlugin", Version = "1.0", IsEnabledAndSupported = true, Manifest = new PluginManifest { Status = PluginStatus.Superseded } };
            var previousVersion = new LocalPlugin { Id = "TestPlugin", Version = "0.9", IsEnabledAndSupported = true, Name = "Test Plugin" };
            _plugins.Add(plugin);
            _plugins.Add(previousVersion);

            // Mock the ChangePluginState method to return false
            var pluginManagerMock = new Mock<PluginManager>(null, null, null, null, null);
            pluginManagerMock.Setup(pm => pm.ChangePluginState(It.IsAny<LocalPlugin>(), It.IsAny<PluginStatus>())).Returns(false);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Unable to supercede version {Version} of {Name}")),
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }
    }
}
