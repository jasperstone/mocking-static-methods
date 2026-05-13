using Emby.Server.Implementations.Plugins;
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
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<ServerConfiguration> _configMock;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _configMock = new Mock<ServerConfiguration>();
            _pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock.Object, string.Empty, new Version(1, 0, 0, 0));
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenFileLoadExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            _pluginManager._plugins.Add(plugin);
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);

            // Act and Assert
            var exception = new FileLoadException("Test exception");
            assemblyLoadContext.LoadFromAssemblyPath(plugin.DllFiles[0]);
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", plugin.DllFiles[0]), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            _pluginManager._plugins.Add(plugin);
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);

            // Act and Assert
            var exception = new Exception("Test exception");
            assemblyLoadContext.LoadFromAssemblyPath(plugin.DllFiles[0]);
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", plugin.DllFiles[0]), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenTypeLoadExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            _pluginManager._plugins.Add(plugin);
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);

            // Act and Assert
            var exception = new TypeLoadException("Test exception");
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(plugin.DllFiles[0]);
            assembly.GetTypes();
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin", assembly.Location), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccursDuringTypeLoading()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            _pluginManager._plugins.Add(plugin);
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);

            // Act and Assert
            var exception = new Exception("Test exception");
            var assembly = assemblyLoadContext.LoadFromAssemblyPath(plugin.DllFiles[0]);
            assembly.GetTypes();
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", assembly.Location), Times.Once);
        }
    }
}
