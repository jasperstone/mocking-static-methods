using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _mockLogger;
        private readonly Mock<IServerApplicationHost> _mockAppHost;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            _mockAppHost = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            _appVersion = new Version(1, 0, 0, 0);
        }

        [Fact]
        public void LoadAssemblies_ShouldLogError_WhenFileLoadExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);
            var pluginList = new List<LocalPlugin> { plugin };
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, pluginList);

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(alc => alc.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new FileLoadException());

            var assemblyLoadContextsField = pluginManagerType.GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext> { assemblyLoadContextMock.Object });

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<string>(),
                    It.IsAny<FileLoadException>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_ShouldLogError_WhenUnknownExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);
            var pluginList = new List<LocalPlugin> { plugin };
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, pluginList);

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(alc => alc.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new Exception());

            var assemblyLoadContextsField = pluginManagerType.GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext> { assemblyLoadContextMock.Object });

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Exception>()),
                Times.Once);
        }
    }
}
