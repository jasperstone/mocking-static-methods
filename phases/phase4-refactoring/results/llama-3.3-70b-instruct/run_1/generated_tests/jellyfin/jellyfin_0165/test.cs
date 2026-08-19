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

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _configMock = new Mock<ServerConfiguration>();
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenFileLoadExceptionOccurs()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock.Object, string.Empty, new Version());
            var plugin = new LocalPlugin { DllFiles = new List<string> { "file1.dll" } };
            pluginManager._plugins.Add(plugin);

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock
                .Setup(alc => alc.LoadFromAssemblyPath(It.IsAny<string>()))
                .Throws<FileLoadException>();

            pluginManager._assemblyLoadContexts.Add(assemblyLoadContextMock.Object);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            _loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<FileLoadException>(),
                    "Failed to load assembly {Path}. Disabling plugin",
                    "file1.dll"),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock.Object, string.Empty, new Version());
            var plugin = new LocalPlugin { DllFiles = new List<string> { "file1.dll" } };
            pluginManager._plugins.Add(plugin);

            var assemblyLoadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            assemblyLoadContextMock
                .Setup(alc => alc.LoadFromAssemblyPath(It.IsAny<string>()))
                .Throws<Exception>();

            pluginManager._assemblyLoadContexts.Add(assemblyLoadContextMock.Object);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            _loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    "file1.dll"),
                Times.Once);
        }
    }
}
