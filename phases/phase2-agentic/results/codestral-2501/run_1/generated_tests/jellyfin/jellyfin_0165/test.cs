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
                Path = _pluginsPath,
                DllFiles = new List<string> { "invalid.dll" }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_ShouldLogError_WhenUnknownExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = _pluginsPath,
                DllFiles = new List<string> { "unknown.dll" }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_ShouldLogError_WhenTypeLoadExceptionOccurs()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                IsEnabledAndSupported = true,
                Path = _pluginsPath,
                DllFiles = new List<string> { "invalidType.dll" }
            };

            var pluginManager = new PluginManager(_mockLogger.Object, _mockAppHost.Object, _config, _pluginsPath, _appVersion);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
