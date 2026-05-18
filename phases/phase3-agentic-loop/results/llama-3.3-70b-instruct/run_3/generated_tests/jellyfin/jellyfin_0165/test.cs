using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenFileLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/non/existent/file.dll" } };
            plugin.IsEnabledAndSupported = true;
            plugin.Path = "path/to/plugin";
            plugin.Version = new Version(1, 0, 0, 0);
            plugin.Name = "Test Plugin";
            plugin.Description = "Test plugin description";
            plugin.Id = Guid.NewGuid();
            plugin.Manifest = new PluginInfo("Test Plugin", new Version(1, 0, 0, 0), "Test plugin description", Guid.NewGuid(), true);
            ((List<LocalPlugin>)pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pluginManager)).Add(plugin);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", "path/to/non/existent/file.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/non/existent/file.dll" } };
            plugin.IsEnabledAndSupported = true;
            plugin.Path = "path/to/plugin";
            plugin.Version = new Version(1, 0, 0, 0);
            plugin.Name = "Test Plugin";
            plugin.Description = "Test plugin description";
            plugin.Id = Guid.NewGuid();
            plugin.Manifest = new PluginInfo("Test Plugin", new Version(1, 0, 0, 0), "Test plugin description", Guid.NewGuid(), true);
            ((List<LocalPlugin>)pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pluginManager)).Add(plugin);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", "path/to/non/existent/file.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenTypeLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version(1, 0, 0, 0));
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/non/existent/file.dll" } };
            plugin.IsEnabledAndSupported = true;
            plugin.Path = "path/to/plugin";
            plugin.Version = new Version(1, 0, 0, 0);
            plugin.Name = "Test Plugin";
            plugin.Description = "Test plugin description";
            plugin.Id = Guid.NewGuid();
            plugin.Manifest = new PluginInfo("Test Plugin", new Version(1, 0, 0, 0), "Test plugin description", Guid.NewGuid(), true);
            ((List<LocalPlugin>)pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pluginManager)).Add(plugin);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin", "path/to/non/existent/file.dll"), Times.Once);
        }
    }
}
