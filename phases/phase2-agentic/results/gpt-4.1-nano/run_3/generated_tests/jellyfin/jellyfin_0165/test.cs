using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerLoggingTests
    {
        private class DummyPlugin : IPlugin
        {
            public string Name { get; set; } = "TestPlugin";
            public string Version { get; set; } = "1.0.0";
            public PluginStatus Status { get; set; } = PluginStatus.Enabled;
            public bool IsEnabledAndSupported => true;
            public string Path { get; set; } = "dummyPath";
            public List<string> DllFiles { get; set; } = new List<string>();
            public PluginManifest Manifest { get; set; } = new PluginManifest();
        }

        [Fact]
        public void LoadAssemblies_Should_LogError_When_FileLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var plugin = new DummyPlugin();
            var pluginList = new List<LocalPlugin>
            {
                new LocalPlugin { Plugin = plugin }
            };

            var pluginManager = new PluginManager(
                loggerMock.Object,
                appHostMock.Object,
                config,
                "plugins",
                new Version(1, 0, 0, 0));

            // Inject the plugin into the internal list
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, pluginList);

            // Mock the PluginLoadContext to throw FileLoadException
            var loadContextMock = new Mock<PluginLoadContext>(plugin.Path);
            loadContextMock.Setup(c => c.LoadFromAssemblyPath(It.IsAny<string>()))
                .Throws(new FileLoadException("Failed to load"));

            // Replace the internal list of contexts with our mock
            var contextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var contexts = new List<AssemblyLoadContext> { loadContextMock.Object };
            contextsField.SetValue(pluginManager, contexts);

            // Act
            var enumerator = pluginManager.LoadAssemblies().GetEnumerator();

            // MoveNext to trigger the loading
            var moved = enumerator.MoveNext();

            // Assert
            // Verify that LogError was called with a FileLoadException
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
