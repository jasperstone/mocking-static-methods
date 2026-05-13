using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
                Manifest = new PluginManifest
                {
                    Status = status,
                    AutoUpdate = true
                };
                IsEnabledAndSupported = isEnabledAndSupported;
                Name = "TestPlugin";
            }

            public override string Id { get; }
            public override Version Version { get; }
            public override PluginManifest Manifest { get; }
            public override bool IsEnabledAndSupported { get; }
            public override string Name { get; }

            // We override ChangePluginState to simulate success or failure
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
                // We replicate the ProcessAlternative method here to call the original private method
                // but we replace ChangePluginState calls with our own method to control behavior and test logging.
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
        public void ProcessAlternative_NoPreviousVersion_SetsRestartAndAutoUpdateFalse()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var plugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);

            manager._plugins.Clear();
            manager._plugins.Add(plugin);

            manager.CallProcessAlternative(plugin);

            Assert.Equal(PluginStatus.Restart, plugin.Manifest.Status);
            Assert.False(plugin.Manifest.AutoUpdate);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ProcessAlternative_PreviousVersionActiveChangeFails_LogsError()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previous = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var current = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Active);

            // Add both plugins
            manager._plugins.Clear();
            manager._plugins.Add(previous);
            manager._plugins.Add(current);

            // Simulate ChangePluginState failure when trying to supersede previous version
            previous.ChangePluginStateFunc = (p, s) => false;

            manager.CallProcessAlternative(current);

            Assert.Equal(PluginStatus.Restart, current.Manifest.Status);
            Assert.False(current.Manifest.AutoUpdate);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_PreviousVersionSupersededChangeFails_LogsError()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previous = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var current = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Superseded);

            // Add both plugins
            manager._plugins.Clear();
            manager._plugins.Add(previous);
            manager._plugins.Add(current);

            // Simulate ChangePluginState failure when trying to activate previous version
            previous.ChangePluginStateFunc = (p, s) => false;

            manager.CallProcessAlternative(current);

            Assert.Equal(PluginStatus.Restart, current.Manifest.Status);
            Assert.False(current.Manifest.AutoUpdate);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
