using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_MakesGetRequestToVersionCheckUrl()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);
            var webServices = new WebServices();
            var game = new Mock<Game>();
            game.Setup(g => g.EngineVersion).Returns("1.0");
            game.Setup(g => g.ModData).Returns(new ModData { Manifest = new ModManifest { Id = "modId", Metadata = new ModMetadata { Version = "1.0" } } });

            // Act
            webServices.CheckModVersion();

            // Assert
            handler.Verify(
                h => h.SendAsync(
                    It.Is<HttpRequestMessage>(rm => rm.Method == HttpMethod.Get && rm.RequestUri.ToString().StartsWith("https://master.openra.net/versioncheck")),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task CheckModVersion_OutdatedResponse_SetsModVersionStatusToOutdated()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("outdated") });
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);
            var webServices = new WebServices();
            var game = new Mock<Game>();
            game.Setup(g => g.EngineVersion).Returns("1.0");
            game.Setup(g => g.ModData).Returns(new ModData { Manifest = new ModManifest { Id = "modId", Metadata = new ModMetadata { Version = "1.0" } } });

            // Act
            webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_UnknownResponse_SetsModVersionStatusToUnknown()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("unknown") });
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);
            var webServices = new WebServices();
            var game = new Mock<Game>();
            game.Setup(g => g.EngineVersion).Returns("1.0");
            game.Setup(g => g.ModData).Returns(new ModData { Manifest = new ModManifest { Id = "modId", Metadata = new ModMetadata { Version = "1.0" } } });

            // Act
            webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Unknown, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_PlaytestResponse_SetsModVersionStatusToPlaytestAvailable()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("playtest") });
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);
            var webServices = new WebServices();
            var game = new Mock<Game>();
            game.Setup(g => g.EngineVersion).Returns("1.0");
            game.Setup(g => g.ModData).Returns(new ModData { Manifest = new ModManifest { Id = "modId", Metadata = new ModMetadata { Version = "1.0" } } });

            // Act
            webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.PlaytestAvailable, webServices.ModVersionStatus);
        }

        [Fact]
        public async Task CheckModVersion_DefaultResponse_SetsModVersionStatusToLatest()
        {
            // Arrange
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var handler = new Mock<HttpMessageHandler>();
            handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("") });
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient.Object);
            var webServices = new WebServices();
            var game = new Mock<Game>();
            game.Setup(g => g.EngineVersion).Returns("1.0");
            game.Setup(g => g.ModData).Returns(new ModData { Manifest = new ModManifest { Id = "modId", Metadata = new ModMetadata { Version = "1.0" } } });

            // Act
            webServices.CheckModVersion();

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }
    }
}
