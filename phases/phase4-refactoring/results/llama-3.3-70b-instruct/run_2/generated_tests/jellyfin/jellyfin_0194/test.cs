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
            var pluginInfo = new PluginInfo("Name", new Version(1, 0, 0, 0), "Description", Guid.NewGuid(), true);
            var plugin = new LocalPlugin(string.Empty, pluginInfo);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
