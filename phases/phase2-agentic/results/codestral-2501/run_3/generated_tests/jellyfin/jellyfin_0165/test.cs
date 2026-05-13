using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
                DllFiles = new List<string> { "path/to/plugin.dll" },
                Manifest = new PluginManifest { Status = PluginStatus.Active }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);
            var pluginList = new List<LocalPlugin> { plugin };
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, pluginList);

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws<FileLoadException>();

            var assemblyLoadContextsField = pluginManagerType.GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext> { assemblyLoadContextMock.Object });

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
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
                DllFiles = new List<string> { "path/to/plugin.dll" },
                Manifest = new PluginManifest { Status = PluginStatus.Active }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);
            var pluginList = new List<LocalPlugin> { plugin };
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, pluginList);

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws<Exception>();

            var assemblyLoadContextsField = pluginManagerType.GetField("_assemblyLoadContexts", BindingFlags.NonPublic | BindingFlags.Instance);
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext> { assemblyLoadContextMock.Object });

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
