using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void ProcessAlternative_ShouldLogError_WhenUnableToSupercede()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, null, new Version(1, 0, 0, 0));

            var plugin = new LocalPlugin
            {
                Id = "TestPlugin",
                Version = new Version(1, 0, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Superseded
                }
            };

            var previousVersion = new LocalPlugin
            {
                Id = "TestPlugin",
                Version = new Version(0, 9, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active
                }
            };

            var plugins = new List<LocalPlugin> { plugin, previousVersion };
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, plugins);

            // Act
            pluginManagerType.GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(pluginManager, new object[] { plugin });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version 0.9.0.0 of TestPlugin")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
