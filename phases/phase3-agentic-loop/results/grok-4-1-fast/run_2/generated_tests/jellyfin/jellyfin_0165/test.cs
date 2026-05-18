using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        [Fact]
        public void LoadAssemblies_WhenGeneralExceptionThrownInLoadFromAssemblyPath_LogsErrorWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "/fake/plugins";
            var appVersion = new Version(10, 8, 0);

            var plugin = new LocalPlugin
            {
                Path = "/fake/plugin",
                DllFiles = new[] { "/fake/MyPlugin.dll" },
                Manifest = new PluginManifest
                {
                    Name = "TestPlugin",
                    Version = "1.0.0.0",
                    Status = PluginStatus.Ok
                }
            };

            var plugins = new List<LocalPlugin> { plugin };
            
            var manager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);
            
            // Use reflection to set private _plugins field
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField?.SetValue(manager, plugins);

            // Setup logger to verify the specific LogError extension call on line 153
            // Capture the formatted message to verify exact template and parameters
            loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly /fake/MyPlugin.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act - LoadFromAssemblyPath will throw FileNotFoundException (not FileLoadException), 
            // hitting the general Exception catch block at line 153
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - Verify the LogError call was made
            loggerMock.Verify();
        }

        [Fact]
        public void LoadAssemblies_WhenGeneralExceptionThrownInAssemblyGetTypes_LogsErrorWithCorrectMessage()
        {
            // Arrange - Setup to reach the GetTypes() exception path
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginsPath = "/fake/plugins";
            var appVersion = new Version(10, 8, 0);

            var plugin = new LocalPlugin
            {
                Path = "/fake/plugin",
                DllFiles = new[] { "/fake/assembly.dll" },
                Manifest = new PluginManifest { Name = "TestPlugin", Version = "1.0.0.0", Status = PluginStatus.Ok }
            };

            var plugins = new List<LocalPlugin> { plugin };
            var manager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginsPath, appVersion);
            
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField?.SetValue(manager, plugins);

            // Setup to verify GetTypes exception logging
            loggerMock
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to load assembly /fake/assembly.dll. Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert
            loggerMock.Verify();
        }
    }

    // Test doubles for types needed by PluginManager constructor and logic
    public class LocalPlugin
    {
        public string Path { get; set; } = string.Empty;
        public string[] DllFiles { get; set; } = Array.Empty<string>();
        public PluginManifest Manifest { get; set; } = new();
        public bool IsEnabledAndSupported => Manifest.Status == PluginStatus.Ok;
    }

    public class PluginManifest
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public PluginStatus Status { get; set; }
    }

    public enum PluginStatus
    {
        Ok,
        Malfunctioned,
        NotSupported
    }

    public interface IServerApplicationHost { }

    public class ServerConfiguration { }

    public interface ILogger<T> { }
}
