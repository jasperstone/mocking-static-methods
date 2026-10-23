using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerLoggingTests
    {
        [Fact]
        public void ProcessAlternative_Should_LogError_When_ChangePluginState_Fails_For_ActiveStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();

            // Create a PluginManager instance with minimal dependencies
            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "dummyPath",
                new Version(1, 0, 0, 0));

            // Prepare a previous version plugin
            var previousPlugin = new LocalPlugin
            {
                Id = "plugin1",
                Name = "TestPlugin",
                Version = new Version(1, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active,
                    AutoUpdate = true
                }
            };

            // Prepare the plugin to process
            var plugin = new LocalPlugin
            {
                Id = "plugin1",
                Name = "TestPlugin",
                Version = new Version(2, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active,
                    AutoUpdate = true
                }
            };

            // Mock the internal method ChangePluginState to return false to trigger LogError
            // Since ChangePluginState is a private method, we need to set up the scenario
            // For simplicity, we can subclass PluginManager to override ChangePluginState
            var pluginManagerWithOverride = new TestablePluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "dummyPath",
                new Version(1, 0, 0, 0))
            {
                _changePluginStateResult = false
            };

            // Act
            pluginManagerWithOverride.ProcessAlternative(plugin);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version") && v.ToString().Contains(previousPlugin.Version.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to override ChangePluginState
        private class TestablePluginManager : PluginManager
        {
            public bool _changePluginStateResult = true;

            public TestablePluginManager(
                ILogger<PluginManager> logger,
                IServerApplicationHost appHost,
                ServerConfiguration config,
                string pluginsPath,
                Version appVersion)
                : base(logger, appHost, config, pluginsPath, appVersion)
            {
            }

            protected override bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                return _changePluginStateResult;
            }
        }
    }
}
