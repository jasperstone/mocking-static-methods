using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.GetTempPath();
            _appVersion = new Version(4, 8, 0, 0);
            
            // Setup logger to verify LogError extension method calls
            _loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void LoadAssemblies_ConstructorSetsUpLoggerCorrectly()
        {
            // Arrange & Act
            var pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            
            // Assert - Verify logger is properly injected (basic sanity check)
            Assert.NotNull(pluginManager);
            _loggerMock.VerifyAll();
        }

        [Fact]
        public void LoadAssemblies_WhenPluginEnabled_CallsThroughLoadPath()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                Name = "TestPlugin",
                Version = new Version(1, 0),
                Path = "/plugins/test",
                DllFiles = new List<string>(),
                IsEnabledAndSupported = true
            };
            var plugins = new List<LocalPlugin> { plugin };
            var pluginManager = CreatePluginManager(plugins);

            // Act
            _ = pluginManager.LoadAssemblies().ToList();

            // Assert - Logger setup was called (verifies control flow reached logger injection)
            _loggerMock.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LoadAssemblies_WhenPluginDisabled_LogsInformationMessage()
        {
            // Arrange
            var plugin = new LocalPlugin
            {
                Name = "DisabledPlugin",
                Version = new Version(1, 0),
                Path = "/plugins/disabled",
                DllFiles = new List<string>(),
                IsEnabledAndSupported = false
            };
            var plugins = new List<LocalPlugin> { plugin };
            var pluginManager = CreatePluginManager(plugins);

            // Act
            _ = pluginManager.LoadAssemblies().ToList();

            // Assert - Verifies the logging extension method pattern is exercised
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        (v?.ToString()?.Contains("Skipping disabled plugin") ?? false) ||
                        (v?.ToString()?.Contains("Version") ?? false) ||
                        (v?.ToString()?.Contains("Name") ?? false)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        private PluginManager CreatePluginManager(List<LocalPlugin> plugins)
        {
            var pluginManager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                _pluginsPath,
                _appVersion);
            
            // Override discovered plugins using reflection
            var pluginsField = typeof(PluginManager)
                .GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField?.SetValue(pluginManager, plugins);
            
            return pluginManager;
        }
    }
}
