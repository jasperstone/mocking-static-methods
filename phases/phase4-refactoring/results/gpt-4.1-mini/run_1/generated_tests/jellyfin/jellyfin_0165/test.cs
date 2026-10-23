using System;
using System.Collections.Generic;
using System.IO;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    // Minimal stub for PluginManifest
    public class PluginManifest
    {
        public PluginStatus Status { get; set; }
    }

    // Minimal stub for LocalPlugin with needed properties
    public class LocalPlugin
    {
        public bool IsEnabledAndSupported { get; set; } = true;
        public Version Version { get; set; } = new Version(1, 0);
        public string Name { get; set; } = "TestPlugin";
        public string Path { get; set; } = "";
        public List<string> DllFiles { get; set; } = new List<string>();
        public PluginManifest Manifest { get; set; } = new PluginManifest();
    }

    // Minimal stub for ServerConfiguration
    public class ServerConfiguration
    {
    }

    public class PluginManagerLogErrorTests
    {
        [Fact]
        public void LoadAssemblies_LogsErrorOnFileLoadException()
        {
            var loggerMock = new Mock<ILogger<PluginManager>>();

            // Create a temporary directory for plugins
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create a dummy plugin directory
                var pluginDir = Path.Combine(tempDir, "plugin1");
                Directory.CreateDirectory(pluginDir);

                // Create a dummy DLL file path that does not exist to cause FileLoadException
                var dummyDllPath = Path.Combine(pluginDir, "nonexistent.dll");

                // Create a PluginManager instance with the plugin directory
                var config = new ServerConfiguration();
                var appVersion = new Version(1, 0);

                var pluginManager = new PluginManager(loggerMock.Object, null!, config, tempDir, appVersion);

                // Use reflection to add a LocalPlugin to the private _plugins list
                var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var pluginsList = (IList<object>)pluginsField!.GetValue(pluginManager)!;

                // Create a LocalPlugin instance with the dummy DLL file path
                var localPlugin = new LocalPlugin
                {
                    Path = pluginDir,
                    DllFiles = new List<string> { dummyDllPath },
                    Manifest = new PluginManifest { Status = PluginStatus.Malfunctioned }
                };

                pluginsList.Add(localPlugin);

                // Call LoadAssemblies and enumerate to trigger loading
                var assemblies = pluginManager.LoadAssemblies();
                foreach (var _ in assemblies) { }

                // Verify LogError was called with FileLoadException
                loggerMock.Verify(
                    x => x.LogError(
                        It.IsAny<FileLoadException>(),
                        "Failed to load assembly {Path}. Disabling plugin",
                        dummyDllPath),
                    Times.Once);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
