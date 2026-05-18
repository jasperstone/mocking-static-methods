using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            _appVersion = new Version(1, 0, 0, 0);
            _pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
        }

        [Fact]
        public void FailPlugin_ShouldLogWarning_WhenPluginNotFound()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            _pluginManager.FailPlugin(assembly);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ShouldLogWarning_WhenInvalidValue()
        {
            // Arrange
            var manifest = new PluginManifest
            {
                // Invalid value to trigger ArgumentException
                Id = null
            };
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            // Act
            var result = _pluginManager.SaveManifest(manifest, path);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
