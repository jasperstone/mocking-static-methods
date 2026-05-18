using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Tests.Plugins
{
    public class PluginManagerTests
    {
        private readonly Mock<ILogger<PluginManager>> _loggerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly ServerConfiguration _config;
        private readonly string _pluginsPath;
        private readonly Version _appVersion;

        public PluginManagerTests()
        {
            _loggerMock = new Mock<ILogger<PluginManager>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _config = new ServerConfiguration();
            _pluginsPath = Path.Combine(Path.GetTempPath(), "PluginsTest");
            Directory.CreateDirectory(_pluginsPath);
            _appVersion = new Version(1, 0, 0, 0);
        }

        [Fact]
        public void SaveManifest_Should_LogWarning_OnArgumentException()
        {
            // Arrange
            var pluginManager = new PluginManager(_loggerMock.Object, _appHostMock.Object, _config, _pluginsPath, _appVersion);
            var invalidPath = Path.Combine(_pluginsPath, "invalid_path");
            var manifest = new PluginManifest();

            // Act
            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest due to invalid value.")),
                    It.IsAny<ArgumentException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
