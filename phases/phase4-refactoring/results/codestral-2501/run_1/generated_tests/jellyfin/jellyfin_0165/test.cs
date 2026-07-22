using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Configuration;
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
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            _mockAppHost = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            _appVersion = new Version(1, 0, 0, 0);
            _pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);
        }

        [Fact]
        public void LoadAssemblies_FileLoadException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" }
            };

            var mockAssemblyLoadContext = new Mock<PluginLoadContext>(plugin.Path);
            mockAssemblyLoadContext.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws<FileLoadException>();

            // Act
            var assemblies = _pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Disabling plugin",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_GeneralException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" }
            };

            var mockAssemblyLoadContext = new Mock<PluginLoadContext>(plugin.Path);
            mockAssemblyLoadContext.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Throws<Exception>();

            // Act
            var assemblies = _pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_TypeLoadException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" }
            };

            var mockAssemblyLoadContext = new Mock<PluginLoadContext>(plugin.Path);
            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(x => x.GetTypes()).Throws<TypeLoadException>();

            mockAssemblyLoadContext.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Returns(mockAssembly.Object);

            // Act
            var assemblies = _pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_ReflectionTypeLoadException_LogsErrorAndDisablesPlugin()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = "path/to/plugin",
                DllFiles = new List<string> { "path/to/plugin.dll" }
            };

            var mockAssemblyLoadContext = new Mock<PluginLoadContext>(plugin.Path);
            var mockAssembly = new Mock<Assembly>();
            mockAssembly.Setup(x => x.GetTypes()).Throws<ReflectionTypeLoadException>();

            mockAssemblyLoadContext.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>())).Returns(mockAssembly.Object);

            // Act
            var assemblies = _pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "Failed to load assembly {Path}. This error occurs when a plugin references an incompatible version of one of the shared libraries. Disabling plugin",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
