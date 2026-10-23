using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly string _testPluginsPath;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _testPluginsPath = Path.Combine(Path.GetTempPath(), "jellyfin_plugins_test_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(_testPluginsPath);
        }

        [Fact]
        public void LoadAssemblies_WhenGeneralExceptionThrownInLoadFromAssemblyPath_LogsErrorWithExpectedMessage()
        {
            // Arrange
            // Create a plugin that will trigger the general Exception catch block (line ~153)
            // by setting up a path that exists but causes LoadFromAssemblyPath to throw non-FileLoadException
            var badDllPath = Path.Combine(_testPluginsPath, "bad.dll");
            File.WriteAllText(badDllPath, "not a valid dll");

            var plugin = new LocalPlugin
            {
                Name = "TestPlugin",
                Version = new Version(1, 0),
                Path = _testPluginsPath,
                DllFiles = new List<string> { badDllPath },
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest { Status = PluginStatus.Ok }
            };

            // Use reflection to inject our test plugin into the private _plugins field
            var manager = new PluginManager(
                _loggerMock.Object, 
                Mock.Of<IServerApplicationHost>(), 
                new ServerConfiguration(), 
                _testPluginsPath, 
                new Version(10, 8, 0));

            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pluginsField?.SetValue(manager, new List<LocalPlugin> { plugin });

            // Act
            var assemblies = manager.LoadAssemblies().ToList();

            // Assert - verify the LogError call from the general Exception catch block (line 153)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(state => 
                        state.ToString().Contains("Failed to load assembly {Path}. Unknown exception was thrown. Disabling plugin") &&
                        state.ToString().Contains(badDllPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testPluginsPath))
                {
                    Directory.Delete(_testPluginsPath, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
