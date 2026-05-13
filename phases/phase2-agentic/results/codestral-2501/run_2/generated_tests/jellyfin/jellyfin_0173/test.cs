using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly PluginManager _pluginManager;
        private readonly List<LocalPlugin> _plugins;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _plugins = new List<LocalPlugin>
            {
                new LocalPlugin
                {
                    DllFiles = new List<string> { "path/to/plugin.dll" },
                    Manifest = new PluginManifest { Status = PluginStatus.Active }
                }
            };
            _pluginManager = new PluginManager(
                _loggerMock.Object,
                Mock.Of<IServerApplicationHost>(),
                new ServerConfiguration(),
                "path/to/plugins",
                new Version(1, 0, 0, 0));
        }

        [Fact]
        public void FailPlugin_PluginNotFound_DoesNotLogWarning()
        {
            // Arrange
            var assembly = new Mock<Assembly>();
            assembly.Setup(a => a.Location).Returns("path/to/unknown.dll");

            // Act
            _pluginManager.FailPlugin(assembly.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()),
                Times.Never);
        }

        [Fact]
        public void FailPlugin_PluginFound_LogsWarning()
        {
            // Arrange
            var assembly = new Mock<Assembly>();
            assembly.Setup(a => a.Location).Returns("path/to/plugin.dll");

            // Act
            _pluginManager.FailPlugin(assembly.Object);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>(), It.IsAny<It.IsAnyType>()),
                Times.Once);
        }
    }
}
