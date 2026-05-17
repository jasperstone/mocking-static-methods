using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Emby.Server.Implementations.Plugins;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
        public void FailPlugin_ShouldLogWarning_WhenPluginIsNotFound()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            _pluginManager.FailPlugin(assembly);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("A plugin's assembly didn't cause this issue, so ignore it.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void SaveManifest_ShouldLogWarning_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var manifest = new PluginManifest();
            var path = "invalid/path";

            // Act
            var result = _pluginManager.SaveManifest(manifest, path);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to save plugin manifest due to invalid value.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task PopulateManifest_ShouldLogError_WhenHttpRequestExceptionIsThrown()
        {
            // Arrange
            var packageInfo = new PackageInfo
            {
                ImageUrl = "http://example.com/image.png",
                Versions = new List<PackageVersionInfo>
                {
                    new PackageVersionInfo { Version = "1.0.0" }
                }
            };
            var version = new Version(1, 0, 0);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            var status = PluginStatus.Active;

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new HttpMessageHandlerMock()));

            _appHostMock.Setup(x => x.Resolve<IHttpClientFactory>())
                .Returns(httpClientFactoryMock.Object);

            // Act
            var result = await _pluginManager.PopulateManifest(packageInfo, version, path, status);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to download image to path")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        private class HttpMessageHandlerMock : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                throw new HttpRequestException("Simulated HTTP request exception");
            }
        }
    }
}
