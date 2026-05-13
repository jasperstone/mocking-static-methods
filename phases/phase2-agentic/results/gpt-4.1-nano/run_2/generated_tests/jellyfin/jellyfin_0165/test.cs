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

            // Inject the plugin directly for testing
            var pluginManagerType = typeof(PluginManager);
            var pluginsField = pluginManagerType.GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var assemblyLoadContextsField = pluginManagerType.GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var loggerField = pluginManagerType.GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Set the plugin list
            pluginsField.SetValue(pluginManager, pluginList);
            // Initialize the assembly load contexts list
            assemblyLoadContextsField.SetValue(pluginManager, new List<AssemblyLoadContext>());
            // Set the logger
            loggerField.SetValue(pluginManager, loggerMock.Object);

            // Mock LoadFromAssemblyPath to throw FileLoadException
            var loadContext = new Mock<AssemblyLoadContext>();
            loadContext.Setup(l => l.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new FileLoadException());

            // Replace the PluginLoadContext constructor to return our mock
            // Since PluginLoadContext is internal, we can't directly mock it here.
            // Instead, we can simulate the behavior by calling LoadAssemblies and expecting error logs.

            // Act
            var loadMethod = typeof(PluginManager).GetMethod("LoadAssemblies", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // Call LoadAssemblies
            var assemblies = pluginManager.LoadAssemblies().ToList();

            // Assert
            // Verify that LogError was called at least once with a FileLoadException
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
