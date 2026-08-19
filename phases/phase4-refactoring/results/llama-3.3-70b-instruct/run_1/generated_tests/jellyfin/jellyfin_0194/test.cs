using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
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
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());
            var plugin = new LocalPlugin(string.Empty, new PluginInfo("TestPlugin", new Version(), Guid.NewGuid(), "Test plugin", "Test author"));
            var previousVersion = new LocalPlugin(string.Empty, new PluginInfo("TestPlugin", new Version(), Guid.NewGuid(), "Test plugin", "Test author"));

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), previousVersion.Version, previousVersion.Name), Times.Once);
        }
    }
}
