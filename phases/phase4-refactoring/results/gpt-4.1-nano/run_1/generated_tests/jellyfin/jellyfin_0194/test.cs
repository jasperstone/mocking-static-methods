using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Plugins;

namespace PluginManagerTests
{
    public class PluginManagerUnitTests
    {
        [Fact]
        public void ProcessAlternative_Should_LogError_When_Cannot_ChangePluginState_From_Active_To_Superseded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PluginManager>>();
            var appHostMock = new Mock<Microsoft.Extensions.Hosting.IHost>();
            var appHost = appHostMock.Object;
            var config = new ServerConfiguration();

            // Create a PluginManager with minimal dependencies
            var pluginManager = new PluginManager(
                loggerMock.Object,
                new Mock<Microsoft.Extensions.Hosting.IHost>().Object,
                config,
                "dummyPath",
                new Version(1, 0, 0, 0));

            // Create a plugin with a higher version
            var plugin = new LocalPlugin
            {
                Id = "test",
                Name = "Test Plugin",
                Version = new Version(1, 0, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active,
                    AutoUpdate = true
                }
            };

            // Add a previous version plugin with lower version
            var previousPlugin = new LocalPlugin
            {
                Id = "test",
                Name = "Test Plugin",
                Version = new Version(0, 9, 0),
                IsEnabledAndSupported = true,
                Manifest = new PluginManifest
                {
                    Status = PluginStatus.Active,
                    AutoUpdate = true
                }
            };

            // Setup plugin list
            pluginManager.GetType().GetField("_plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(pluginManager, new List<LocalPlugin> { previousPlugin, plugin });

            // Act
            // Call ProcessAlternative with the plugin that has a different version
            var processMethod = typeof(PluginManager).GetMethod("ProcessAlternative", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processMethod.Invoke(pluginManager, new object[] { plugin });

            // Assert
            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to enable version") && v.ToString().Contains(previousPlugin.Version.ToString())),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
