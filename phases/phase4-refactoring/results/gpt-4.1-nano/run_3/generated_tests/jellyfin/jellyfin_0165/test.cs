using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using Emby.Server.Implementations.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerTest
    {
        [Fact]
        public void LoadAssemblies_Should_LogError_When_ExceptionOccursDuringAssemblyLoad()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Mock plugin with a DllFile that will cause exception
            var plugin = new LocalPlugin
            {
                Name = "TestPlugin",
                Version = new Version(1, 0, 0),
                Path = Path.GetTempPath(),
                DllFiles = new List<string> { "nonexistent.dll" },
                Manifest = new PluginManifest { Status = PluginStatus.Enabled }
            };

            // Use reflection to set _plugins field
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginsList = new List<LocalPlugin> { plugin };
            pluginsField.SetValue(pluginManager, pluginsList);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", "nonexistent.dll"),
                Times.AtLeastOnce);
        }
    }
}
