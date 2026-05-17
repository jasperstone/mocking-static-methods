using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerTest
    {
        private class DummyPlugin : LocalPlugin
        {
            public DummyPlugin()
            {
                Id = "testId";
                Name = "TestPlugin";
                Version = new Version(1, 0, 0);
                IsEnabledAndSupported = true;
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active,
                    AutoUpdate = true
                };
                Path = "dummyPath";
                DllFiles = new List<string> { "dummy.dll" };
            }
        }

        [Fact]
        public void ProcessAlternative_Should_LogError_When_UnableToChangePluginState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();

            var pluginManager = new TestPluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "plugins",
                new Version(10, 0, 0));

            var plugin = new DummyPlugin();

            // Setup plugin list with a previous version
            var previousVersion = new DummyPlugin
            {
                Version = new Version(0, 9, 0),
                Name = "PreviousVersion"
            };
            var pluginsList = new List<LocalPlugin> { previousVersion, plugin };
            pluginManager.SetPlugins(pluginsList);

            // Force plugin status to Active
            plugin.Manifest.Status = PluginStatus.Active;

            // Mock ChangePluginState to return false to simulate failure
            pluginManager.SetChangePluginStateResult(false);

            // Act
            pluginManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Unable to enable version {Version} of {Name}", previousVersion.Version, previousVersion.Name),
                Times.Once);
        }

        private class TestPluginManager : PluginManager
        {
            private bool _changePluginStateResult = true;
            private List<LocalPlugin> _plugins = new List<LocalPlugin>();

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
                _plugins = plugins;
                var field = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, _plugins);
            }

            public void SetChangePluginStateResult(bool result)
            {
                _changePluginStateResult = result;
            }

            protected override bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                return _changePluginStateResult;
            }
        }
    }
}
