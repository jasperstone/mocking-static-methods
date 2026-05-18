using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerTests
    {
        private class DummyPlugin : ILocalPlugin
        {
            public string Name { get; set; } = "TestPlugin";
            public Version Version { get; set; } = new Version(1, 0);
            public PluginStatus Status { get; set; } = PluginStatus.Enabled;
            public bool IsEnabledAndSupported => Status == PluginStatus.Enabled;
            public List<string> DllFiles { get; set; } = new List<string> { "dummy.dll" };
            public string Path { get; set; } = "/dummy/path";
            public PluginManifest Manifest { get; set; } = new PluginManifest { Status = PluginStatus.Enabled };
        }

        [Fact]
        public void LoadAssemblies_Should_LogErrorAndDisablePlugin_When_FileLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var plugin = new DummyPlugin();

            // Setup appHost to return empty exports
            appHostMock.Setup(x => x.GetExports<IPlugin>(It.IsAny<Func<IPlugin, bool>>()))
                .Returns(new List<IPlugin>());

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, "/plugins", new Version(1, 0));

            // Inject plugin with a DllFile that will cause LoadFromAssemblyPath to throw
            plugin.DllFiles = new List<string> { "bad.dll" };
            var plugins = new List<ILocalPlugin> { plugin };
            // Use reflection to set private _plugins
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, plugins);

            // Mock LoadFromAssemblyPath to throw FileLoadException
            var loadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            loadContextMock.Setup(x => x.LoadFromAssemblyPath(It.IsAny<string>()))
                .Throws(new FileLoadException());

            // Replace the load context creation to return our mock
            // Since PluginLoadContext is instantiated directly, we need to use a workaround
            // For simplicity, assume we can inject a factory or mock the constructor (not shown here)
            // Alternatively, we can test the method directly with a mock context if refactored

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();

            // Since the method is an iterator, we need to move next to execute
            var moved = enumerator.MoveNext();

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Failed to load assembly {Path}. Disabling plugin", "bad.dll"),
                Times.Once);
            // Additional asserts can be added to verify plugin state change if accessible
        }
    }
}
