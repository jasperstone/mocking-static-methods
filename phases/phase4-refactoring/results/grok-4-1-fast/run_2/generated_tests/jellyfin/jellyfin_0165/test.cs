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
    public class PluginManagerTests : IDisposable
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<object> _appHostMock;
        private readonly object _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;
        private PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<object>();
            _config = new object();
            _pluginsPath = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N")[..8]);
            _appVersion = new Version(10, 8, 0);
            
            Directory.CreateDirectory(_pluginsPath);
        }

        [Fact]
        public void LoadAssemblies_WhenGeneralExceptionThrownInLoadFromAssemblyPath_LogsErrorWithUnknownExceptionMessage()
        {
            // Arrange - create plugin manager with a plugin that has non-existent DLL
            var plugin = CreateMinimalPlugin();
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            _pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            pluginsField?.SetValue(_pluginManager, new List<object> { plugin });

            // Act
            var act = () => _pluginManager.LoadAssemblies().ToList();

            // Assert - missing DLL throws FileNotFoundException which is caught by general Exception handler (line 153)
            act.Invoke();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to load assembly") &&
                        v.ToString()!.Contains(_pluginsPath) &&
                        v.ToString()!.Contains("Unknown exception was thrown. Disabling plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void LoadAssemblies_WhenFileLoadExceptionThrown_LogsErrorWithoutUnknownExceptionMessage()
        {
            // Arrange - same setup triggers FileLoadException path first
            var plugin = CreateMinimalPlugin();
            var pluginsField = typeof(PluginManager).GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            _pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            pluginsField?.SetValue(_pluginManager, new List<object> { plugin });

            // Act
            var act = () => _pluginManager.LoadAssemblies().ToList();

            // Assert
            act.Invoke();

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("Failed to load assembly") &&
                        v.ToString()!.Contains(_pluginsPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        private object CreateMinimalPlugin()
        {
            // Create minimal plugin object using reflection to match internal structure
            var plugin = new { 
                IsEnabledAndSupported = true,
                Path = _pluginsPath,
                DllFiles = new[] { Path.Combine(_pluginsPath, "nonexistent.dll") }
            };
            return plugin;
        }

        public void Dispose()
        {
            _pluginManager?.Dispose();
            if (Directory.Exists(_pluginsPath))
            {
                try
                {
                    Directory.Delete(_pluginsPath, true);
                }
                catch { }
            }
        }
    }
}
