using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerLoggingTests
    {
        [Fact]
        public void LoadAssemblies_LogsError_WhenAssemblyLoadFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = Environment.CurrentDirectory;
            var appVersion = new Version(1, 0);

            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);

            // Create a LocalPlugin with a DllFile path that will fail to load
            var plugin = new LocalPluginStub
            {
                Id = "plugin1",
                Version = new Version(1, 0),
                Manifest = new PluginManifestStub { Status = PluginStatus.Active },
                IsEnabledAndSupported = true,
                Name = "TestPlugin",
                DllFiles = new List<string> { "nonexistent.dll" },
                Path = pluginsPath
            };

            // Set _plugins field via reflection
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField.SetValue(pluginManager, new List<LocalPlugin> { plugin });

            // Act
            var assemblies = pluginManager.LoadAssemblies();

            // Force enumeration to trigger loading
            foreach (var _ in assemblies) { }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to load assembly")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class LocalPluginStub : LocalPlugin
        {
            public override string Id { get; set; }
            public override Version Version { get; set; }
            public override PluginManifest Manifest { get; set; }
            public override bool IsEnabledAndSupported { get; set; }
            public override string Name { get; set; }
            public override List<string> DllFiles { get; set; }
            public override string Path { get; set; }
        }

        private class PluginManifestStub : PluginManifest
        {
            public override PluginStatus Status { get; set; }
        }
    }
}
