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

namespace Emby.Server.Tests
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
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);
            _pluginManager._plugins.Add(plugin);

            // Act and Assert
            assemblyLoadContext.LoadFromAssemblyPath = (path) => throw new FileLoadException("Test exception");
            _pluginManager.LoadAssemblies();
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", "path/to/assembly.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);
            _pluginManager._plugins.Add(plugin);

            // Act and Assert
            assemblyLoadContext.LoadFromAssemblyPath = (path) => throw new Exception("Test exception");
            _pluginManager.LoadAssemblies();
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", "path/to/assembly.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenTypeLoadExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);
            _pluginManager._plugins.Add(plugin);
            var assembly = new Assembly();
            assemblyLoadContext.LoadFromAssemblyPath = (path) => assembly;

            // Act and Assert
            assembly.GetTypes = () => throw new TypeLoadException("Test exception");
            _pluginManager.LoadAssemblies();
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin", "path/to/assembly.dll"), Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccursDuringTypeLoading()
        {
            // Arrange
            var plugin = new LocalPlugin { DllFiles = new List<string> { "path/to/assembly.dll" } };
            var assemblyLoadContext = new PluginLoadContext(plugin.Path);
            _pluginManager._assemblyLoadContexts.Add(assemblyLoadContext);
            _pluginManager._plugins.Add(plugin);
            var assembly = new Assembly();
            assemblyLoadContext.LoadFromAssemblyPath = (path) => assembly;

            // Act and Assert
            assembly.GetTypes = () => throw new Exception("Test exception");
            _pluginManager.LoadAssemblies();
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin", "path/to/assembly.dll"), Times.Once);
        }
    }
}
