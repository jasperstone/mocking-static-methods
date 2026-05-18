using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
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
        public void ProcessAlternative_ShouldLogError_WhenChangePluginStateFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "path/to/plugins";
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            var plugin = new LocalPlugin("path/to/plugin", true, new PluginManifest
            {
                Id = Guid.NewGuid(),
                Name = "TestPlugin",
                Version = "1.0.0",
                Status = PluginStatus.Active
            });

            var previousVersion = new LocalPlugin("path/to/previousVersion", true, new PluginManifest
            {
                Id = plugin.Id,
                Name = "TestPlugin",
                Version = "0.9.0",
                Status = PluginStatus.Superseded
            });

            var plugins = new List<LocalPlugin> { plugin, previousVersion };
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, plugins);

            // Mock ChangePluginState to return false
            var pluginManagerMock = new Mock<PluginManager>(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);
            pluginManagerMock.Setup(m => m.ChangePluginState(It.IsAny<LocalPlugin>(), It.IsAny<PluginStatus>())).Returns(false);

            // Act
            var processAlternativeMethod = pluginManagerType.GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processAlternativeMethod.Invoke(pluginManager, new object[] { plugin });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version 0.9.0 of TestPlugin")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
