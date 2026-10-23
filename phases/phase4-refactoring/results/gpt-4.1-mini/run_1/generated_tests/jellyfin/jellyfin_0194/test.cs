using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            public TestLocalPlugin(string id, Version version, PluginStatus status, bool isEnabledAndSupported)
            {
                Id = id;
                Version = version;
                Manifest = new PluginManifest { Status = status, AutoUpdate = true };
                IsEnabledAndSupported = isEnabledAndSupported;
                Name = $"Plugin-{id}";
            }

            public override string Id { get; }
            public override Version Version { get; }
            public override PluginManifest Manifest { get; }
            public override bool IsEnabledAndSupported { get; }
            public override string Name { get; }
            public override IReadOnlyList<string> DllFiles => Array.Empty<string>();
            public override string Path => "";

            // Simulate ChangePluginState behavior
            public Func<PluginStatus, bool>? ChangePluginStateFunc { get; set; }

            public override bool ChangePluginState(PluginStatus status)
            {
                if (ChangePluginStateFunc != null)
                {
                    return ChangePluginStateFunc(status);
                }
                return true;
            }
        }

        [Fact]
        public void LoadAssemblies_LogsError_When_ChangePluginStateFails_ActiveStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new PluginManager(loggerMock.Object, null!, null!, "", new Version(1, 0));

            var previousPlugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active, true);
            var currentPlugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Active, true);

            // Setup ChangePluginState to fail for previousPlugin
            previousPlugin.ChangePluginStateFunc = status => false;

            // Set private _plugins field via reflection
            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(manager, new List<LocalPlugin> { previousPlugin, currentPlugin });

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoadAssemblies_LogsError_When_ChangePluginStateFails_SupersededStatus()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var manager = new PluginManager(loggerMock.Object, null!, null!, "", new Version(1, 0));

            var previousPlugin = new TestLocalPlugin("plugin1", new Version(1, 0), PluginStatus.Active, true);
            var currentPlugin = new TestLocalPlugin("plugin1", new Version(2, 0), PluginStatus.Superseded, true);

            // Setup ChangePluginState to fail for previousPlugin
            previousPlugin.ChangePluginStateFunc = status => false;

            // Set private _plugins field via reflection
            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance);
            pluginsField.SetValue(manager, new List<LocalPlugin> { previousPlugin, currentPlugin });

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to supercede version")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for LocalPlugin and PluginManifest to compile
    public abstract class LocalPlugin
    {
        public abstract string Id { get; }
        public abstract Version Version { get; }
        public abstract PluginManifest Manifest { get; }
        public abstract bool IsEnabledAndSupported { get; }
        public abstract string Name { get; }
        public abstract IReadOnlyList<string> DllFiles { get; }
        public abstract string Path { get; }
        public virtual bool ChangePluginState(PluginStatus status) => true;
    }

    public class PluginManifest
    {
        public PluginStatus Status { get; set; }
        public bool AutoUpdate { get; set; }
    }
}
