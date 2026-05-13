using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace PluginManagerTests
{
    public class ProcessAlternativeTests
    {
        [Fact]
        public void LogError_IsCalled_When_ChangePluginState_ReturnsFalse_For_ActiveStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginPath = "dummyPath";
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                pluginPath,
                appVersion);

            // Create a plugin with status Active
            var plugin = new LocalPlugin
            {
                Manifest = new PluginManifest { Status = PluginStatus.Active, AutoUpdate = true },
                Version = new Version(1, 0, 0),
                Name = "TestPlugin",
                Id = "TestId",
                Path = "dummyPath",
                DllFiles = new List<string> { "dummy.dll" }
            };

            // Create a previous version plugin with different version
            var previousVersionPlugin = new LocalPlugin
            {
                Manifest = new PluginManifest { Status = PluginStatus.Active, AutoUpdate = true },
                Version = new Version(0, 9, 0),
                Name = "TestPlugin",
                Id = "TestId",
                Path = "dummyPath",
                DllFiles = new List<string> { "dummy.dll" }
            };

            // Setup the plugin list
            var plugins = new List<LocalPlugin> { plugin, previousVersionPlugin };

            // Use reflection to set the private _plugins field
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, plugins);

            // Setup ChangePluginState to return false
            var changePluginStateMethod = typeof(PluginManager).GetMethod("ChangePluginState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // We will mock ChangePluginState to always return false
            // But since it's a private method, we can't directly mock it.
            // Instead, we can subclass PluginManager to override it for testing.

            var testPluginManager = new TestPluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                pluginPath,
                appVersion);

            // Assign the same plugins list
            testPluginManager.SetPlugins(plugins);

            // Act
            testPluginManager.ProcessAlternative(plugin);

            // Assert
            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version") && v.ToString().Contains(previousVersionPlugin.Version.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to override ChangePluginState
        private class TestPluginManager : PluginManager
        {
            private List<LocalPlugin> _testPlugins;

            public TestPluginManager(
                ILogger<PluginManager> logger,
                IServerApplicationHost appHost,
                ServerConfiguration config,
                string pluginsPath,
                Version appVersion)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
            }

            public void SetPlugins(List<LocalPlugin> plugins)
            {
                var field = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, plugins);
            }

            protected override bool ChangePluginState(LocalPlugin plugin, PluginStatus newStatus)
            {
                // Simulate failure for testing
                return false;
            }
        }
    }
}
