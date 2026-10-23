using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly PluginManager _pluginManager;
        private readonly List<LocalPlugin> _pluginsField;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _loggerMock.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception, string>>((level, eventId, state, ex, formatter) => 
                {
                    if (state is Microsoft.Extensions.Logging.FormattedLogValues formattedLogValues)
                    {
                        Console.WriteLine($"LOG: {formatter(state, ex)}");
                    }
                });

            var pluginsPath = "/fake/plugins/path";
            var appVersion = new Version(10, 8, 0, 0);
            
            // Create mocks for missing dependencies
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();

            _pluginManager = new PluginManager(
                _loggerMock.Object,
                appHostMock.Object,
                config,
                pluginsPath,
                appVersion);

            // Use reflection to replace the private _plugins field
            var pluginsField = typeof(PluginManager).GetField("_plugins", BindingFlags.NonPublic | BindingFlags.Instance)!;
            _pluginsField = new List<LocalPlugin>();
            pluginsField.SetValue(_pluginManager, _pluginsField);
        }

        [Fact]
        public void ProcessAlternative_SupersededPlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var plugin = Mocks.CreateLocalPlugin("test-plugin", new Version(2, 0, 0, 0), PluginStatus.Superseded);
            var previousVersion = Mocks.CreateLocalPlugin("test-plugin", new Version(1, 0, 0, 0), PluginStatus.Active);

            _pluginsField.Add(plugin);
            _pluginsField.Add(previousVersion);

            // Mock ChangePluginState to return false by setting status back after change
            var changePluginStateMethod = typeof(PluginManager).GetMethod("ChangePluginState", BindingFlags.NonPublic | BindingFlags.Instance)!;
            changePluginStateMethod.Invoke(_pluginManager, new object[] { previousVersion, PluginStatus.Active });
            
            // Reset status to Active so ChangePluginState returns false
            previousVersion.Manifest.Status = PluginStatus.Active;

            // Act
            var processAlternativeMethod = typeof(PluginManager).GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance)!;
            processAlternativeMethod.Invoke(_pluginManager, new object[] { plugin });

            // Assert - verify the specific LogError call (line 905 equivalent)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Unable to supercede version 1.0.0.0 of test-plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_ActivePlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var plugin = Mocks.CreateLocalPlugin("test-plugin", new Version(2, 0, 0, 0), PluginStatus.Active);
            var previousVersion = Mocks.CreateLocalPlugin("test-plugin", new Version(1, 0, 0, 0), PluginStatus.Active);

            _pluginsField.Add(plugin);
            _pluginsField.Add(previousVersion);

            // Mock ChangePluginState to return false by setting status back after change
            var changePluginStateMethod = typeof(PluginManager).GetMethod("ChangePluginState", BindingFlags.NonPublic | BindingFlags.Instance)!;
            changePluginStateMethod.Invoke(_pluginManager, new object[] { previousVersion, PluginStatus.Superseded });
            
            // Reset status to Active so ChangePluginState returns false
            previousVersion.Manifest.Status = PluginStatus.Active;

            // Act
            var processAlternativeMethod = typeof(PluginManager).GetMethod("ProcessAlternative", BindingFlags.NonPublic | BindingFlags.Instance)!;
            processAlternativeMethod.Invoke(_pluginManager, new object[] { plugin });

            // Assert - verify the first LogError call
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("Unable to enable version 1.0.0.0 of test-plugin")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Nested mocks class to avoid type resolution issues
    internal static class Mocks
    {
        public static LocalPlugin CreateLocalPlugin(string id, Version version, PluginStatus status)
        {
            var manifest = new PluginManifest
            {
                Id = id,
                Version = version.ToString(),
                Name = id,
                Status = status
            };

            return new LocalPlugin
            {
                Id = id,
                Version = version,
                Name = id,
                Manifest = manifest,
                Path = "/fake/path",
                DllFiles = new List<string>()
            };
        }
    }
}
