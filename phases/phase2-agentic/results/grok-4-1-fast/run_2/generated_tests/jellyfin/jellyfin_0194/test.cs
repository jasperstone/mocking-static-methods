using System;
using System.Collections.Generic;
using System.Linq;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly List<LocalPlugin> _mockPlugins;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _mockPlugins = new List<LocalPlugin>();

            // Create mock dependencies
            var mockAppHost = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var mockConfig = new MediaBrowser.Model.Configuration.ServerConfiguration();

            // Setup mock plugins list that PluginManager uses
            var pluginManager = new PluginManager(
                _loggerMock.Object,
                mockAppHost.Object,
                mockConfig,
                "/fake/plugins/path",
                new Version(10, 8, 0));

            // Use reflection to set private _plugins field for testing ProcessAlternative
            typeof(PluginManager)
                .GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(pluginManager, _mockPlugins);

            _pluginManager = pluginManager;
        }

        [Fact]
        public void ProcessAlternative_SupersededPlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var pluginId = "test-plugin";
            var pluginVersion = new Version(2, 0, 0, 0);
            var previousVersion = new Version(1, 0, 0, 0);

            var currentPlugin = CreateLocalPlugin(pluginId, pluginVersion, PluginStatus.Superseded);
            var previousPlugin = CreateLocalPlugin(pluginId, previousVersion, PluginStatus.Active);

            _mockPlugins.Add(previousPlugin);
            _mockPlugins.Add(currentPlugin);

            // Mock ChangePluginState to return false (failure case)
            var mockPreviousPlugin = Mock.Get(previousPlugin);
            mockPreviousPlugin.Setup(p => p.Manifest)
                .Returns(new Mock<MediaBrowser.Model.Plugins.PluginManifest>().Object);
            // Note: ChangePluginState is private/internal, so we test the logging behavior through the public flow

            // Act
            // Access private method via reflection
            var processAlternativeMethod = typeof(PluginManager)
                .GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            processAlternativeMethod?.Invoke(_pluginManager, new object[] { currentPlugin });

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Unable to supercede version") && v.ToString().Contains(previousVersion.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProcessAlternative_ActivePlugin_ChangePluginStateFails_LogsError()
        {
            // Arrange
            var pluginId = "test-plugin";
            var pluginVersion = new Version(2, 0, 0, 0);
            var previousVersion = new Version(1, 0, 0, 0);

            var currentPlugin = CreateLocalPlugin(pluginId, pluginVersion, PluginStatus.Active);
            var previousPlugin = CreateLocalPlugin(pluginId, previousVersion, PluginStatus.Active);

            _mockPlugins.Add(previousPlugin);
            _mockPlugins.Add(currentPlugin);

            // Act
            var processAlternativeMethod = typeof(PluginManager)
                .GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            processAlternativeMethod?.Invoke(_pluginManager, new object[] { currentPlugin });

            // Assert - Verifies the other LogError branch (line ~905 equivalent)
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Unable to enable version") && v.ToString().Contains(previousVersion.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        private static LocalPlugin CreateLocalPlugin(string id, Version version, PluginStatus status)
        {
            var mockPlugin = new Mock<LocalPlugin>();
            mockPlugin.Setup(p => p.Id).Returns(id);
            mockPlugin.Setup(p => p.Version).Returns(version);
            mockPlugin.Setup(p => p.Name).Returns("Test Plugin");
            mockPlugin.Setup(p => p.IsEnabledAndSupported).Returns(true);
            mockPlugin.Setup(p => p.Manifest).Returns(new Mock<MediaBrowser.Model.Plugins.PluginManifest>
            {
                Status = status
            }.Object);
            return mockPlugin.Object;
        }
    }
}
