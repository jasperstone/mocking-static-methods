using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Plugins;

namespace Emby.Server.Implementations.Plugins.Tests
{
    public sealed class PluginManagerTests : IDisposable
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly PluginManager _pluginManager;
        private bool _disposed;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            string pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0);

            _pluginManager = new PluginManager(
                _loggerMock.Object,
                _appHostMock.Object,
                _config,
                pluginsPath,
                appVersion);
        }

        [Fact]
        public void SaveManifest_WhenArgumentExceptionThrown_LogsWarningAndReturnsFalse()
        {
            // Arrange
            var manifest = new PluginManifest();
            string path = "invalid:path"; // Invalid path to cause ArgumentException on File.WriteAllText

            // Act
            bool result = _pluginManager.SaveManifest(manifest, path);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest due to invalid value.")),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _pluginManager.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
