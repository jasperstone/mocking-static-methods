using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Logging;

namespace PluginManagerTests
{
    public class PluginManagerTest
    {
        [Fact]
        public void LoadAssemblies_Should_LogError_When_FileLoadExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginPath = "testPath";
            var appVersion = new Version(1, 0, 0, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginPath, appVersion);

            // Create a dummy plugin with a DllFile that will cause LoadFromAssemblyPath to throw
            var plugin = new LocalPlugin
            {
                Name = "TestPlugin",
                Version = "1.0",
                Path = pluginPath,
                DllFiles = new List<string> { "dummy.dll" },
                Manifest = new PluginManifest { Status = PluginStatus.Enabled }
            };

            // Use reflection to set the private _plugins list
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pluginsList = new List<LocalPlugin> { plugin };
            pluginsField.SetValue(pluginManager, pluginsList);

            // Mock LoadFromAssemblyPath to throw FileLoadException
            var loadContext = new Mock<PluginLoadContext>(pluginPath);
            loadContext.Setup(lc => lc.LoadFromAssemblyPath(It.IsAny<string>())).Throws(new FileLoadException());

            // Since constructor is used directly, we need to replace the _assemblyLoadContexts list with our mock
            var assemblyContextsField = typeof(PluginManager).GetField("_assemblyLoadContexts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var contextsList = new List<AssemblyLoadContext> { loadContext.Object };
            assemblyContextsField.SetValue(pluginManager, contextsList);

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Assert
            // Verify that LogError was called with a FileLoadException
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly dummy.dll")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}
