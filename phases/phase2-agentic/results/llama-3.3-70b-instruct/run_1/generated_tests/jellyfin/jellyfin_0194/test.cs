using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Emby.Server.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<ServerConfiguration> _configMock;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _configMock = new Mock<ServerConfiguration>();
            _pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock.Object, string.Empty, new Version(1, 0, 0, 0));
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFails()
        {
            // Arrange
            var plugin = new LocalPlugin { Id = "id", Version = new Version(1, 0, 0, 0), Name = "name", IsEnabledAndSupported = true, Manifest = new PluginManifest { Status = PluginStatus.Active } };
            var previousVersion = new LocalPlugin { Id = "id", Version = new Version(1, 0, 0, 0), Name = "name", IsEnabledAndSupported = true, Manifest = new PluginManifest { Status = PluginStatus.Active } };

            _pluginManager._plugins.Add(plugin);
            _pluginManager._plugins.Add(previousVersion);

            // Act
            _pluginManager.ProcessAlternative(plugin);

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), previousVersion.Version, previousVersion.Name), Times.Once);
        }
    }
}
