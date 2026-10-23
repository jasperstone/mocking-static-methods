using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
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

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<ILogger> _loggerMock2;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IJsonSerializer> _jsonSerializerMock;
        private readonly ServerConfiguration _configMock;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _loggerMock2 = new Mock<ILogger>();
            _fileSystemMock = new Mock<IFileSystem>();
            _jsonSerializerMock = new Mock<IJsonSerializer>();
            _configMock = new ServerConfiguration(_appPathsMock.Object, _loggerMock2.Object, _fileSystemMock.Object, _jsonSerializerMock.Object);
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            _appVersion = new Version(1, 0, 0, 0);
        }

        [Fact]
        public async Task LoadAssemblies_LogsError_WhenAssemblyLoadFails()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock, _pluginsPath, _appVersion);
            var plugin = new LocalPlugin { Name = "TestPlugin", Version = "1.0.0", Path = _pluginsPath, DllFiles = new List<string> { "TestPlugin.dll" } };
            pluginManager._plugins.Add(plugin);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LoadAssemblies_LogsError_WhenTypeLoadFails()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _configMock, _pluginsPath, _appVersion);
            var plugin = new LocalPlugin { Name = "TestPlugin", Version = "1.0.0", Path = _pluginsPath, DllFiles = new List<string> { "TestPlugin.dll" } };
            pluginManager._plugins.Add(plugin);

            // Act
            pluginManager.LoadAssemblies();

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin", It.IsAny<string>()), Times.Once);
        }
    }
}
