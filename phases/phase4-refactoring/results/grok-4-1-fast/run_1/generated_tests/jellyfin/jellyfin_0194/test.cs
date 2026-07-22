using System;
using System.Collections.Generic;
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
        [Fact]
        public void ProcessAlternative_SupersededPlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange - Tests line 905 specifically (_logger.LogError("Unable to supercede version {Version}..."))
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugins = new List<LocalPlugin>();
            
            var plugin = new LocalPlugin
            {
                Id = "test-plugin",
                Version = new Version(2, 0),
                Name = "Test Plugin",
                Manifest = new PluginManifest { Status = PluginStatus.Superseded }
            };
            
            var previousVersion = new LocalPlugin
            {
                Id = "test-plugin",
                Version = new Version(1, 0),
                Name = "Test Plugin",
                IsEnabledAndSupported = true
            };
            
            plugins.Add(previousVersion);

            var pluginManager = new PluginManager(loggerMock.Object, null!, null!, "", new Version(1, 0));
            
            // Inject plugins list via reflection
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, plugins);

            // Use reflection to call private ProcessAlternative method
            var processAlternativeMethod = typeof(PluginManager).GetMethod("ProcessAlternative", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act - This will trigger the LogError on line 905 when ChangePluginState returns false
            processAlternativeMethod.Invoke(pluginManager, new[] { plugin });

            // Assert - Verify LogError was called with correct message parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state.ToString()!.Contains("Unable to supercede version 1.0") && 
                        state.ToString()!.Contains("Test Plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_ActivePlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var plugins = new List<LocalPlugin>();
            
            var plugin = new LocalPlugin
            {
                Id = "test-plugin",
                Version = new Version(2, 0),
                Name = "Test Plugin",
                Manifest = new PluginManifest { Status = PluginStatus.Active }
            };
            
            var previousVersion = new LocalPlugin
            {
                Id = "test-plugin",
                Version = new Version(1, 0),
                Name = "Test Plugin",
                IsEnabledAndSupported = true
            };
            
            plugins.Add(previousVersion);

            var pluginManager = new PluginManager(loggerMock.Object, null!, null!, "", new Version(1, 0));
            
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            pluginsField.SetValue(pluginManager, plugins);

            var processAlternativeMethod = typeof(PluginManager).GetMethod("ProcessAlternative", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act
            processAlternativeMethod.Invoke(pluginManager, new[] { plugin });

            // Assert - Verify the other LogError call was made
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        state.ToString()!.Contains("Unable to enable version 1.0") && 
                        state.ToString()!.Contains("Test Plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
