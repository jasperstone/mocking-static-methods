using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));

            var plugin = new LocalPlugin(string.Empty, new PluginInfo("Test Plugin", new Version(1, 0, 0, 0), "Test Plugin", Guid.NewGuid(), true));
            plugin.Manifest.Status = PluginStatus.Active;

            var previousVersion = new LocalPlugin(string.Empty, new PluginInfo("Test Plugin", new Version(1, 0, 0, 0), "Test Plugin", Guid.NewGuid(), true));

            pluginManager._plugins.Add(plugin);
            pluginManager._plugins.Add(previousVersion);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), previousVersion.Version, previousVersion.Name), Times.Once);
        }
    }
}
