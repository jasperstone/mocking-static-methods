using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Updates;
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
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();

            // Create minimal setup for constructor
            var pluginsPath = "/fake/plugins";
            var appVersion = new Version(10, 8, 0);

            _pluginManager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                pluginsPath,
                appVersion);
        }

        [Fact]
        public void ProcessAlternative_SupersededPlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var plugin = CreateLocalPlugin("test-plugin", new Version(2, 0));
            plugin.Manifest.Status = PluginStatus.Superseded;

            var previousVersion = CreateLocalPlugin("test-plugin", new Version(1, 0));
            previousVersion.Manifest.Status = PluginStatus.Active;

            // Mock the plugins list to return previousVersion
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginsList = new List<LocalPlugin> { previousVersion };
            pluginsField?.SetValue(_pluginManager, pluginsList);

            // Mock ChangePluginState to return false
            var changePluginStateMethod = typeof(PluginManager).GetMethod("ChangePluginState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            changePluginStateMethod?.CreateDelegate(typeof(Func<LocalPlugin, PluginStatus, bool>), (Func<LocalPlugin, PluginStatus, bool>)(_, __) => false)
                .DynamicInvoke();

            // Act
            var processMethod = typeof(PluginManager).GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processMethod?.Invoke(_pluginManager, new object[] { plugin });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Unable to supercede version 1.0 of test-plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_ActivePlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var plugin = CreateLocalPlugin("test-plugin", new Version(2, 0));
            plugin.Manifest.Status = PluginStatus.Active;

            var previousVersion = CreateLocalPlugin("test-plugin", new Version(1, 0));
            previousVersion.Manifest.Status = PluginStatus.Active;

            // Mock the plugins list to return previousVersion
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginsList = new List<LocalPlugin> { previousVersion };
            pluginsField?.SetValue(_pluginManager, pluginsList);

            // Mock ChangePluginState to return false
            var changePluginStateMethod = typeof(PluginManager).GetMethod("ChangePluginState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var processMethod = typeof(PluginManager).GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processMethod?.Invoke(_pluginManager, new object[] { plugin });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Unable to enable version 1.0 of test-plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        private static LocalPlugin CreateLocalPlugin(string id, Version version)
        {
            var manifest = new PluginManifest
            {
                Id = id,
                Name = "Test Plugin",
                Version = version.ToString(),
                Status = PluginStatus.Active,
                AutoUpdate = true
            };

            return new LocalPlugin
            {
                Id = id,
                Name = "Test Plugin",
                Version = version,
                Manifest = manifest,
                Path = "/fake/path",
                DllFiles = new List<string> { "/fake.dll" },
                IsEnabledAndSupported = true
            };
        }
    }
}
