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

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.IServerApplicationHost> _appHostMock;
        private readonly Mock<MediaBrowser.Model.Configuration.ServerConfiguration> _configMock;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            _configMock = new Mock<MediaBrowser.Model.Configuration.ServerConfiguration>();
            _pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock.Object, string.Empty, new Version(1, 0, 0, 0));
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFails()
        {
            // Arrange
            var plugin = new LocalPlugin(string.Empty, true, new PluginManifest());
            var previousVersion = new LocalPlugin(string.Empty, true, new PluginManifest());
            _pluginManager.ProcessAlternative(plugin);

            // Act
            _pluginManager.ProcessAlternative(plugin);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
