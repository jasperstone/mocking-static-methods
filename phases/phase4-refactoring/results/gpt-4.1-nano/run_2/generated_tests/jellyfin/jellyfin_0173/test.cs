using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Threading.Tasks;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.IO;
using System;

namespace PluginManagerTests
{
    public class PluginManagerLoggingTests
    {
        [Fact]
        public void SaveManifest_Should_LogWarning_On_ArgumentException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var config = new ServerConfiguration();
            var pluginPath = "testPath";
            var appVersion = new Version(1, 0, 0, 0);
            var pluginManager = new PluginManager(loggerMock.Object, appHostMock.Object, config, pluginPath, appVersion);

            // Use reflection to set private _logger field to a mock that captures logs
            var loggerField = typeof(PluginManager).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(pluginManager, loggerMock.Object);

            // Create a dummy manifest
            var manifest = new PluginManifest();

            // Act
            // Call SaveManifest with invalid path to trigger ArgumentException
            var result = pluginManager.SaveManifest(manifest, null);

            // Assert
            // Verify that LogWarning was called with expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest due to invalid value.")),
                    It.IsAny<ArgumentException>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
