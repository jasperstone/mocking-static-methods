using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _mockLogger;
        private readonly PluginManager _pluginManager;
        private readonly List<LocalPlugin> _plugins;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Create PluginManager with minimal dependencies using mocks/objects
            var fakeAppHost = new { Resolve = new Func<Type, object>(t => null) };
            var fakeConfig = new { };
            
            _pluginManager = new PluginManager(
                _mockLogger.Object,
                fakeAppHost,
                fakeConfig,
                "/fake/path",
                new Version(1, 0, 0, 0));

            // Replace internal plugins list to control test data
            var pluginsField = typeof(PluginManager).GetField("_plugins", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            _plugins = new List<LocalPlugin>();
            pluginsField.SetValue(_pluginManager, _plugins);
        }

        [Fact]
        public void ProcessAlternative_SupersededPlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var plugin = CreateLocalPlugin("test-plugin", new Version(2, 0), PluginStatus.Superseded);
            var previousVersion = CreateLocalPlugin("test-plugin", new Version(1, 0), PluginStatus.Active);
            previousVersion.IsEnabledAndSupported = true;
            _plugins.Add(previousVersion);

            // Force ChangePluginState to return false by making it private/unmockable - test will hit error path naturally

            // Act
            InvokeProcessAlternative(plugin);

            // Assert - verify the specific LogError call from line 905
            _mockLogger.Verify(x => x.LogError(
                "Unable to supercede version {Version} of {Name}", 
                previousVersion.Version, 
                previousVersion.Name), 
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_ActivePlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var plugin = CreateLocalPlugin("test-plugin", new Version(2, 0), PluginStatus.Active);
            var previousVersion = CreateLocalPlugin("test-plugin", new Version(1, 0), PluginStatus.Active);
            previousVersion.IsEnabledAndSupported = true;
            _plugins.Add(previousVersion);

            // Act
            InvokeProcessAlternative(plugin);

            // Assert - verify the first LogError call
            _mockLogger.Verify(x => x.LogError(
                "Unable to enable version {Version} of {Name}",
                previousVersion.Version,
                previousVersion.Name),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_NoPreviousVersion_DoesNotLogError()
        {
            // Arrange
            var plugin = CreateLocalPlugin("unique-plugin", new Version(1, 0), PluginStatus.Active);

            // Act
            InvokeProcessAlternative(plugin);

            // Assert - no LogError calls
            _mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        private static LocalPlugin CreateLocalPlugin(string id, Version version, PluginStatus status)
        {
            return new LocalPlugin
            {
                Id = id,
                Version = version,
                Name = "Test Plugin",
                Manifest = new PluginManifest { Status = status },
                Path = "/fake/path",
                DllFiles = new List<string>()
            };
        }

        private void InvokeProcessAlternative(LocalPlugin plugin)
        {
            var method = typeof(PluginManager).GetMethod("ProcessAlternative", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(_pluginManager, new object[] { plugin });
        }
    }
}
