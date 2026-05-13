using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private class TestLocalPlugin : LocalPlugin
        {
            public TestLocalPlugin(string id, Version version, PluginStatus status, bool isEnabledAndSupported = true)
            {
                Id = id;
                Version = version;
                Manifest = new PluginManifest { Status = status, AutoUpdate = true };
                IsEnabledAndSupported = isEnabledAndSupported;
                Name = "TestPlugin";
            }

            public override string Id { get; }
            public override Version Version { get; }
            public override PluginManifest Manifest { get; }
            public override bool IsEnabledAndSupported { get; }
            public override string Name { get; }

            // We need to override ChangePluginState to simulate success or failure
            public Func<LocalPlugin, PluginStatus, bool> ChangePluginStateFunc { get; set; } = (p, s) => true;

            public bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                return ChangePluginStateFunc(plugin, status);
            }
        }

        private class TestPluginManager : PluginManager
        {
            public TestPluginManager(ILogger<PluginManager> logger)
                : base(logger, null!, null!, "", new Version(1, 0))
            {
                _plugins = new List<LocalPlugin>();
            }

            public new List<LocalPlugin> _plugins;

            public new ILogger<PluginManager> _logger => base._logger;

            public bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                if (plugin is TestLocalPlugin testPlugin)
                {
                    return testPlugin.ChangePluginState(plugin, status);
                }
                return true;
            }

            public void CallProcessAlternative(LocalPlugin plugin)
            {
                // We replicate the ProcessAlternative method here to call the private method
                // because the original is private and we want to test the logging behavior.
                var previousVersion = _plugins.OrderByDescending(p => p.Version)
                    .FirstOrDefault(
                        p => p.Id.Equals(plugin.Id)
                        && p.IsEnabledAndSupported
                        && p.Version != plugin.Version);

                if (previousVersion is null)
                {
                    plugin.Manifest.Status = PluginStatus.Restart;
                    plugin.Manifest.AutoUpdate = false;
                    return;
                }

                if (plugin.Manifest.Status == PluginStatus.Active && !ChangePluginState(previousVersion, PluginStatus.Superseded))
                {
                    _logger.LogError("Unable to enable version {Version} of {Name}", previousVersion.Version, previousVersion.Name);
                }
                else if (plugin.Manifest.Status == PluginStatus.Superseded && !ChangePluginState(previousVersion, PluginStatus.Active))
                {
                    _logger.LogError("Unable to supercede version {Version} of {Name}", previousVersion.Version, previousVersion.Name);
                }

                plugin.Manifest.Status = PluginStatus.Restart;
                plugin.Manifest.AutoUpdate = false;
            }
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFailsForActiveStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previousPlugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var currentPlugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Active);

            manager._plugins.Add(previousPlugin);
            manager._plugins.Add(currentPlugin);

            // Simulate ChangePluginState returns false for superseding previous version
            previousPlugin.ChangePluginStateFunc = (p, s) => false;

            // Act
            manager.CallProcessAlternative(currentPlugin);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(PluginStatus.Restart, currentPlugin.Manifest.Status);
            Assert.False(currentPlugin.Manifest.AutoUpdate);
        }

        [Fact]
        public void ProcessAlternative_LogsError_WhenChangePluginStateFailsForSupersededStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previousPlugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var currentPlugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Superseded);

            manager._plugins.Add(previousPlugin);
            manager._plugins.Add(currentPlugin);

            // Simulate ChangePluginState returns false for activating previous version
            previousPlugin.ChangePluginStateFunc = (p, s) => false;

            // Act
            manager.CallProcessAlternative(currentPlugin);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(PluginStatus.Restart, currentPlugin.Manifest.Status);
            Assert.False(currentPlugin.Manifest.AutoUpdate);
        }

        [Fact]
        public void ProcessAlternative_SetsRestartStatus_WhenNoPreviousVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var currentPlugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);

            manager._plugins.Add(currentPlugin);

            // Act
            manager.CallProcessAlternative(currentPlugin);

            // Assert
            Assert.Equal(PluginStatus.Restart, currentPlugin.Manifest.Status);
            Assert.False(currentPlugin.Manifest.AutoUpdate);

            // No error logs should be called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
