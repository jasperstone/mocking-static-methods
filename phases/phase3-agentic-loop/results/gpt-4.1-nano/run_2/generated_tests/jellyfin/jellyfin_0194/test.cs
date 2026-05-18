using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerTest
    {
        private class DummyPlugin : LocalPlugin
        {
            public DummyPlugin(string id, string name, Version version, PluginStatus status, bool isEnabled)
            {
                Id = id;
                Name = name;
                Version = version;
                Manifest = new PluginManifest { Status = status, AutoUpdate = true };
                IsEnabledAndSupported = isEnabled;
                DllFiles = new List<string>();
                Path = "";
            }
        }

        [Fact]
        public void ProcessAlternative_Should_LogError_When_UnableToChangePluginState()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "plugins",
                new Version(1, 0, 0, 0));

            var plugin = new DummyPlugin("id1", "TestPlugin", new Version(1, 0), PluginStatus.Active, true);
            var previousVersion = new DummyPlugin("id1", "TestPlugin", new Version(0, 9), PluginStatus.Active, true);

            // Setup ChangePluginState to return false to simulate failure
            var pluginManagerType = typeof(PluginManager);
            var changeMethod = pluginManagerType.GetMethod("ChangePluginState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // We can't directly mock private methods, so we will use reflection to invoke ProcessAlternative with a mock ChangePluginState that returns false.
            // Instead, we will subclass PluginManager to override ChangePluginState for testing.

            var testManager = new TestPluginManager(loggerMock.Object, appHostMock.Object, config);
            testManager.AddPlugin(plugin);
            testManager.AddPlugin(previousVersion);

            // Act
            testManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Unable to enable version {Version} of {Name}", previousVersion.Version, previousVersion.Name),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_Should_LogError_When_SupercedeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var testManager = new TestPluginManager(loggerMock.Object, appHostMock.Object, config);

            var plugin = new DummyPlugin("id2", "TestPlugin2", new Version(1, 0), PluginStatus.Superseded, true);
            var previousVersion = new DummyPlugin("id2", "TestPlugin2", new Version(0, 9), PluginStatus.Active, true);

            testManager.AddPlugin(plugin);
            testManager.AddPlugin(previousVersion);

            // Override ChangePluginState to return false for supercede attempt
            testManager.SetChangePluginStateResult(false);

            // Act
            testManager.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                x => x.LogError("Unable to supercede version {Version} of {Name}", previousVersion.Version, previousVersion.Name),
                Times.Once);
        }

        // Helper subclass to override ChangePluginState for testing
        private class TestPluginManager : PluginManager
        {
            private bool _changeResult = true;
            public TestPluginManager(ILogger<PluginManager> logger, IServerApplicationHost appHost, ServerConfiguration config)
                : base(logger, appHost, config, "plugins", new Version(1, 0))
            {
                _plugins = new List<LocalPlugin>();
            }

            public void AddPlugin(LocalPlugin plugin)
            {
                _plugins.Add(plugin);
            }

            public void SetChangePluginStateResult(bool result)
            {
                _changeResult = result;
            }

            protected override bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                return _changeResult;
            }
        }
    }
}
