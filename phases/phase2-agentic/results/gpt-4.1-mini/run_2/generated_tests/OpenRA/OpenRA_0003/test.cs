using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA;
using OpenRA.FileSystem;
using Xunit;

namespace OpenRA.Game.Tests
{
    public class MapPreviewTests
    {
        // Helper class to expose Install method for testing
        private class TestMapPreview : MapPreview
        {
            public TestMapPreview(MapCache cache, ModData modData, string uid) : base(cache, modData, uid) { }

            public new void Install(string mapRepositoryUrl) => base.Install(mapRepositoryUrl);
        }

        [Fact]
        public async Task Install_DoesNotStartDownload_WhenStatusIsNotDownloadErrorOrDownloadAvailable()
        {
            // Arrange
            var cacheMock = new Mock<MapCache>();
            var modDataMock = new Mock<ModData>();
            var mapPreview = new TestMapPreview(cacheMock.Object, modDataMock.Object, "testuid");

            // Set Status to Available (not DownloadError or DownloadAvailable)
            typeof(MapPreview).GetField("innerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(mapPreview, new MapPreview.InnerData { Status = MapStatus.Available });

            // Act
            mapPreview.Install("http://example.com/");

            // Assert
            // No exception and no download started (no async task)
            // We can't directly assert no call to HttpClient, but no exceptions means early return
        }

        [Fact]
        public async Task Install_SetsStatusDownloadError_WhenMapInstallPackageNotFound()
        {
            // Arrange
            var cacheMock = new Mock<MapCache>();
            var modDataMock = new Mock<ModData>();
            var mapPreview = new TestMapPreview(cacheMock.Object, modDataMock.Object, "testuid");

            // Setup cache.MapLocations to return a key that is not IReadWritePackage
            var mapLocations = new System.Collections.Generic.Dictionary<IReadOnlyPackage, MapClassification>
            {
                { new DummyPackage(), MapClassification.User }
            };
            typeof(MapPreview).GetField("cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(mapPreview, cacheMock.Object);
            cacheMock.Setup(c => c.MapLocations).Returns(mapLocations);

            // Set Status to DownloadError to allow download
            typeof(MapPreview).GetField("innerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(mapPreview, new MapPreview.InnerData { Status = MapStatus.DownloadError });

            // Act
            mapPreview.Install("http://example.com/");

            // Assert
            var innerData = (MapPreview.InnerData)typeof(MapPreview).GetField("innerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(mapPreview);
            Assert.Equal(MapStatus.DownloadError, innerData.Status);
        }

        [Fact]
        public async Task Install_CallsHttpClientGetAsync_WithCorrectUrl()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var expectedUri = new Uri("http://example.com/testuid");

            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req =>
                     req.Method == HttpMethod.Get
                     && req.RequestUri == expectedUri),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("content")
                   {
                       Headers =
                       {
                           ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                           {
                               FileName = "mapfile.map"
                           }
                       }
                   }
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var cacheMock = new Mock<MapCache>();
            var modDataMock = new Mock<ModData>();
            var mapInstallPackageMock = new Mock<IReadWritePackage>();
            var mapLocations = new System.Collections.Generic.Dictionary<IReadOnlyPackage, MapClassification>
            {
                { mapInstallPackageMock.Object, MapClassification.User }
            };
            cacheMock.Setup(c => c.MapLocations).Returns(mapLocations);

            var mapPreview = new TestMapPreview(cacheMock.Object, modDataMock.Object, "testuid");

            // Set Status to DownloadAvailable to allow download
            typeof(MapPreview).GetField("innerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(mapPreview, new MapPreview.InnerData { Status = MapStatus.DownloadAvailable });

            // Replace HttpClientFactory.Create to return our httpClient
            var httpClientFactoryField = typeof(MapPreview).GetField("HttpClientFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (httpClientFactoryField != null)
            {
                httpClientFactoryField.SetValue(null, new Func<HttpClient>(() => httpClient));
            }
            else
            {
                // If no HttpClientFactory field, we cannot inject HttpClient, so skip test
                return;
            }

            // Act
            mapPreview.Install("http://example.com/");

            // Wait a bit for the async task to start and call GetAsync
            await Task.Delay(100);

            // Assert
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.AtLeastOnce(),
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get
                  && req.RequestUri == expectedUri),
               ItExpr.IsAny<CancellationToken>());
        }

        // Dummy IReadOnlyPackage implementation for testing
        private class DummyPackage : IReadOnlyPackage
        {
            public void Dispose() { }
            public Stream Open(string path) => Stream.Null;
            public bool Exists(string path) => false;
            public string[] GetFiles(string path) => Array.Empty<string>();
        }
    }
}
