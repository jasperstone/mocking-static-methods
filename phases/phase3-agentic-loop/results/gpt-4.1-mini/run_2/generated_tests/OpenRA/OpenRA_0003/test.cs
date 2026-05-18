using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA;
using OpenRA.FileSystem;
using Xunit;

namespace OpenRA.Game.Tests.Map
{
    public class MapPreviewTests
    {
        // We create a derived class to override HttpClient creation for testing
        private class TestableMapPreview : MapPreview
        {
            private readonly HttpClient _httpClient;
            private readonly MapStatus _initialStatus;
            private readonly bool _allowDownloading;

            public TestableMapPreview(HttpClient httpClient, MapStatus initialStatus, bool allowDownloading)
            {
                _httpClient = httpClient;
                _initialStatus = initialStatus;
                _allowDownloading = allowDownloading;

                // Setup innerData with initial status
                var innerDataField = typeof(MapPreview).GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
                var innerDataType = typeof(MapPreview).GetNestedType("InnerData", BindingFlags.NonPublic);
                var innerData = Activator.CreateInstance(innerDataType);
                innerDataType.GetField("Status").SetValue(innerData, _initialStatus);
                innerDataField.SetValue(this, innerData);

                // Setup cache with a User classification and a mock IReadWritePackage
                var cacheField = typeof(MapPreview).GetField("cache", BindingFlags.NonPublic | BindingFlags.Instance);
                var cacheMock = new Mock<MapCache>();
                var packageMock = new Mock<IReadWritePackage>();
                packageMock.Setup(p => p.Update(It.IsAny<string>(), It.IsAny<byte[]>())).Verifiable();
                packageMock.Setup(p => p.OpenPackage(It.IsAny<string>(), It.IsAny<ModFiles>())).Returns((IReadOnlyPackage)null);

                var mapLocations = new Dictionary<IReadWritePackage, MapClassification>
                {
                    { packageMock.Object, MapClassification.User }
                };
                cacheMock.SetupGet(c => c.MapLocations).Returns(mapLocations);
                cacheField.SetValue(this, cacheMock.Object);

                // Setup Game.Settings.Game.AllowDownloading to _allowDownloading
                var gameField = typeof(MapPreview).GetField("Game", BindingFlags.NonPublic | BindingFlags.Instance);
                var gameMock = new Mock<IGame>();
                var settingsMock = new Mock<ISettings>();
                var gameSettingsMock = new Mock<IGameSettings>();
                gameSettingsMock.SetupGet(s => s.AllowDownloading).Returns(_allowDownloading);
                settingsMock.SetupGet(s => s.Game).Returns(gameSettingsMock.Object);
                gameMock.SetupGet(g => g.Settings).Returns(settingsMock.Object);
                gameField.SetValue(this, gameMock.Object);

                // Setup Uid field
                var uidField = typeof(MapPreview).GetField("Uid", BindingFlags.Public | BindingFlags.Instance);
                uidField.SetValue(this, "mapUid");
            }

            protected override HttpClient CreateHttpClient()
            {
                return _httpClient;
            }
        }

        [Fact]
        public async Task Install_WhenStatusIsDownloadAvailableAndAllowDownloadingTrue_CallsHttpClientGetAsync()
        {
            // Arrange
            var expectedUrl = "http://example.com/mapUid";

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
            };
            responseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "mapfile.map"
            };

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var mapPreview = new TestableMapPreview(httpClient, MapStatus.DownloadAvailable, true);

            // Act
            mapPreview.Install("http://example.com/");

            // Wait some time for the Task.Run to complete
            await Task.Delay(200);

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.AtLeastOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void Install_WhenStatusIsNotDownloadAvailableOrDownloadError_DoesNotCallHttpClient()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(handlerMock.Object);

            var mapPreview = new TestableMapPreview(httpClient, MapStatus.Available, true);

            // Act
            mapPreview.Install("http://example.com/");

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void Install_WhenAllowDownloadingIsFalse_DoesNotCallHttpClient()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(handlerMock.Object);

            var mapPreview = new TestableMapPreview(httpClient, MapStatus.DownloadAvailable, false);

            // Act
            mapPreview.Install("http://example.com/");

            // Assert
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }

    // Interfaces and classes to support mocking and compilation
    public interface IGame
    {
        ISettings Settings { get; }
    }

    public interface ISettings
    {
        IGameSettings Game { get; }
    }

    public interface IGameSettings
    {
        bool AllowDownloading { get; }
    }

    public class MapCache
    {
        public virtual IDictionary<IReadWritePackage, MapClassification> MapLocations { get; }
    }

    public interface IReadWritePackage : IReadOnlyPackage
    {
        void Update(string filename, byte[] data);
    }

    public interface IReadOnlyPackage
    {
        IReadOnlyPackage OpenPackage(string filename, ModFiles modFiles);
    }

    public class ModFiles { }

    // Partial MapPreview class to allow overriding HttpClient creation
    public partial class MapPreview
    {
        protected virtual HttpClient CreateHttpClient()
        {
            // Use new HttpClient by default for production
            return new HttpClient();
        }

        public void Install(string mapRepositoryUrl)
        {
            if ((Status != MapStatus.DownloadError && Status != MapStatus.DownloadAvailable) || !Game.Settings.Game.AllowDownloading)
                return;

            innerData.Status = MapStatus.Downloading;
            var installLocation = cache.MapLocations.FirstOrDefault(p => p.Value == MapClassification.User);
            if (installLocation.Key is not IReadWritePackage mapInstallPackage)
            {
                Log.Write("debug", "Map install directory not found");
                innerData.Status = MapStatus.DownloadError;
                return;
            }

            Task.Run(async () =>
            {
                var mapUrl = mapRepositoryUrl + Uid;
                try
                {
                    void OnDownloadProgress(long total, long received, int percentage)
                    {
                        // No-op for test
                    }

                    var client = CreateHttpClient();

                    var response = await client.GetAsync(mapUrl, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        innerData.Status = MapStatus.DownloadError;
                        return;
                    }

                    var mapFilename = response.Content.Headers.ContentDisposition?.FileName;

                    if (string.IsNullOrEmpty(mapFilename))
                    {
                        innerData.Status = MapStatus.DownloadError;
                        return;
                    }

                    var fileStream = new MemoryStream();

                    await response.Content.CopyToAsync(fileStream);

                    mapInstallPackage.Update(mapFilename, fileStream.ToArray());
                    Log.Write("debug", $"Downloaded map to '{mapFilename}'");

                    var p = mapInstallPackage.OpenPackage(mapFilename, modData.ModFiles);
                    if (p == null)
                        innerData.Status = MapStatus.DownloadError;
                    else
                        UpdateFromMapWithoutOwningPackage(p, mapInstallPackage, MapClassification.User, GridType);
                }
                catch (Exception e)
                {
                    Log.Write("debug", e.ToString());
                    innerData.Status = MapStatus.DownloadError;
                }
            });
        }
    }

    public static class Log
    {
        public static void Write(string level, string message) { }
        public static void Write(string level, Exception e) { }
    }
}
