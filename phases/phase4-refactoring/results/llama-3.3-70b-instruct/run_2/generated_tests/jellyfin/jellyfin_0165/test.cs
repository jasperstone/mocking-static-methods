using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenFileLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());
            var plugin = new LocalPlugin { DllFiles = new List<string> { "non-existent-assembly.dll" } };
            var privateField = pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", "non-existent-assembly.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());
            var plugin = new LocalPlugin { DllFiles = new List<string> { "non-existent-assembly.dll" } };
            var privateField = pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", "non-existent-assembly.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenTypeLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var pluginManager = new PluginManager(loggerMock.Object, null, null, string.Empty, new Version());
            var plugin = new LocalPlugin { DllFiles = new List<string> { "non-existent-assembly.dll" } };
            var privateField = pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin", "non-existent-assembly.dll"), Times.Once);
        }
    }
}
