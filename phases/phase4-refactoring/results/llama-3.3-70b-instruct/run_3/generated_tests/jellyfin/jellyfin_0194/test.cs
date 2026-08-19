using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var plugin = new LocalPlugin(string.Empty, new PluginInfo("TestPlugin", new Version(1, 0, 0, 0), "TestDescription", Guid.NewGuid(), true));
            plugin.IsEnabledAndSupported = true;
            plugin.Manifest.Status = PluginStatus.Active;

            var previousVersion = new LocalPlugin(string.Empty, new PluginInfo("TestPlugin", new Version(1, 0, 0, 0), "TestDescription", Guid.NewGuid(), true));
            previousVersion.IsEnabledAndSupported = true;
            previousVersion.Manifest.Status = PluginStatus.Active;

            var pluginsField = pluginManager.GetType().GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPlugin>());

            var processAlternativeMethod = pluginManager.GetType().GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance);
            processAlternativeMethod.Invoke(pluginManager, new[] { plugin });

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), previousVersion.Version, previousVersion.Name), Times.Once);
        }
    }
}
