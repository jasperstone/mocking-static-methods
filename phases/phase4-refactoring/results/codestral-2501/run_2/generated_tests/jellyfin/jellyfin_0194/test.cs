using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_ShouldLogError_WhenUnableToSupercedeVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "path/to/plugins";
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var plugin = new PluginInfo("TestPlugin", new Version(1, 0, 0, 0), "Test Plugin", Guid.NewGuid(), true)
            {
                Status = PluginStatus.Superseded
            };

            var previousVersion = new PluginInfo("TestPlugin", new Version(0, 9, 0, 0), "Test Plugin", Guid.NewGuid(), true)
            {
                Status = PluginStatus.Active
            };

            var plugins = new List<PluginInfo> { plugin, previousVersion };

            // Mock the _plugins property
            var pluginManagerMock = new Mock<PluginManager>(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);
            pluginManagerMock.Setup(pm => pm.Plugins).Returns(plugins);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "Unable to supercede version {Version} of {Name}",
                    previousVersion.Version,
                    previousVersion.Name),
                Times.Once);
        }
    }
}
