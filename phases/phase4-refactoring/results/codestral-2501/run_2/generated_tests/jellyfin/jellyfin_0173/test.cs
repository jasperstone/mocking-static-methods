using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public interface IServerApplicationHostWrapper
    {
        IHttpClientFactory ResolveHttpClientFactory();
    }

    public class ServerApplicationHostWrapper : IServerApplicationHostWrapper
    {
        private readonly IServerApplicationHost _appHost;

        public ServerApplicationHostWrapper(IServerApplicationHost appHost)
        {
            _appHost = appHost;
        }

        public IHttpClientFactory ResolveHttpClientFactory()
        {
            return _appHost.Resolve<IHttpClientFactory>();
        }
    }

    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _mockLogger;
        private readonly Mock<IServerApplicationHostWrapper> _mockAppHostWrapper;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;
        private readonly PluginManager _pluginManager;

        public PluginManagerTests()
        {
            _mockLogger = new Mock<ILogger<PluginManager>>();
            _mockAppHostWrapper = new Mock<IServerApplicationHostWrapper>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            _appVersion = new Version(1, 0, 0, 0);
            _pluginManager = new PluginManager(_mockLogger.Object, _mockAppHostWrapper.Object, _config, _pluginsPath, _appVersion);
        }

        [Fact]
        public void FailPlugin_ShouldChangePluginStateToMalfunctioned_WhenPluginExists()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();
            var plugin = new LocalPlugin
            {
                DllFiles = new List<string> { assembly.Location },
                Manifest = new PluginManifest { Status = PluginStatus.Active }
            };
            _pluginManager.Plugins.Add(plugin);

            // Act
            _pluginManager.FailPlugin(assembly);

            // Assert
            Assert.Equal(PluginStatus.Malfunctioned, plugin.Manifest.Status);
        }

        [Fact]
        public void FailPlugin_ShouldNotChangePluginState_WhenPluginDoesNotExist()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            _pluginManager.FailPlugin(assembly);

            // Assert
            Assert.Empty(_pluginManager.Plugins);
        }

        [Fact]
        public void SaveManifest_ShouldLogWarning_WhenManifestIsInvalid()
        {
            // Arrange
            var manifest = new PluginManifest();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

            // Act
            var result = _pluginManager.SaveManifest(manifest, path);

            // Assert
            _mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);
            Assert.False(result);
        }
    }
}
