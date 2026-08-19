using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Emby.Server.Implementations.Plugins;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PluginManagerTests
{
    public class SaveManifestLoggingTests
    {
        [Fact]
        public void SaveManifest_Should_LogWarning_On_ArgumentException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PluginManager>>();
            var mockAppHost = new Mock<Microsoft.Extensions.Hosting.IHost>();
            var config = new MediaBrowser.Model.Configuration.ServerConfiguration();
            var pluginsPath = Path.GetTempPath();
            var appVersion = new Version(1, 0, 0, 0);

            // Mock the IServerApplicationHost to return a service provider that can resolve IHttpClientFactory
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IHttpClientFactory)))
                .Returns(mockHttpClientFactory.Object);
            var mockServerHost = new Mock<IServerApplicationHost>();
            mockServerHost.Setup(sh => sh.Resolve<IHttpClientFactory>())
                .Returns(mockHttpClientFactory.Object);
            // Setup the mock appHost to return the mock server host
            var mockHost = new Mock<Microsoft.Extensions.Hosting.IHost>();
            mockHost.Setup(h => h.Services).Returns(new ServiceCollection()
                .AddSingleton(mockServerHost.Object)
                .BuildServiceProvider());

            var pluginManager = new PluginManager(
                mockLogger.Object,
                mockServerHost.Object,
                config,
                pluginsPath,
                appVersion);

            // Create a valid PluginManifest object
            var manifest = new PluginManifest
            {
                Name = "TestPlugin",
                Version = "1.0.0",
                Status = PluginStatus.Enabled
            };

            // Act
            // Pass an invalid path to cause ArgumentException in File.WriteAllText
            var invalidPath = Path.Combine("\0", "invalid");

            var result = pluginManager.SaveManifest(manifest, invalidPath);

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<ArgumentException>(),
                    "Unable to save plugin manifest due to invalid value. {Path}",
                    invalidPath),
                Times.Once);
        }
    }
}
