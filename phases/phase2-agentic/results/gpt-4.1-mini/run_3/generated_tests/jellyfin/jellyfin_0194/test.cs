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
        }

        private class TestPluginManager : PluginManager
        {
            public TestPluginManager(ILogger<PluginManager> logger)
                : base(logger, null!, null!, "", new Version(1, 0))
            {
                // Override _plugins list for testing
                _plugins = new List<LocalPlugin>();
            }

            public new List<LocalPlugin> _plugins;

            public bool ChangePluginStateReturnValue = true;

            protected override bool ChangePluginState(LocalPlugin plugin, PluginStatus status)
            {
                return ChangePluginStateReturnValue;
            }

            public void CallProcessAlternative(LocalPlugin plugin)
            {
                // Call private method via reflection
                var method = typeof(PluginManager).GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(this, new object[] { plugin });
            }
        }

        [Fact]
        public void ProcessAlternative_NoPreviousVersion_SetsRestartAndAutoUpdateFalse()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var plugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);

            manager._plugins.Add(plugin);

            // No other versions, so previousVersion is null
            manager.CallProcessAlternative(plugin);

            Assert.Equal(PluginStatus.Restart, plugin.Manifest.Status);
            Assert.False(plugin.Manifest.AutoUpdate);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ProcessAlternative_PreviousVersionChangePluginStateFails_LogsErrorForActiveStatus()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previousVersion = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var plugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Active);

            manager._plugins.Add(previousVersion);
            manager._plugins.Add(plugin);

            // Simulate ChangePluginState returns false
            manager.ChangePluginStateReturnValue = false;

            manager.CallProcessAlternative(plugin);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(PluginStatus.Restart, plugin.Manifest.Status);
            Assert.False(plugin.Manifest.AutoUpdate);
        }

        [Fact]
        public void ProcessAlternative_PreviousVersionChangePluginStateFails_LogsErrorForSupersededStatus()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previousVersion = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var plugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Superseded);

            manager._plugins.Add(previousVersion);
            manager._plugins.Add(plugin);

            // Simulate ChangePluginState returns false
            manager.ChangePluginStateReturnValue = false;

            manager.CallProcessAlternative(plugin);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(PluginStatus.Restart, plugin.Manifest.Status);
            Assert.False(plugin.Manifest.AutoUpdate);
        }

        [Fact]
        public void ProcessAlternative_PreviousVersionChangePluginStateSucceeds_DoesNotLogError()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new TestPluginManager(loggerMock.Object);

            var previousVersion = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active);
            var plugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Active);

            manager._plugins.Add(previousVersion);
            manager._plugins.Add(plugin);

            // Simulate ChangePluginState returns true
            manager.ChangePluginStateReturnValue = true;

            manager.CallProcessAlternative(plugin);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);

            Assert.Equal(PluginStatus.Restart, plugin.Manifest.Status);
            Assert.False(plugin.Manifest.AutoUpdate);
        }
    }
}
