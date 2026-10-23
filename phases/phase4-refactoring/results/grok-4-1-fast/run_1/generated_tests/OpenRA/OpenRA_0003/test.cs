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

namespace OpenRA.Tests.Map
{
	public class MapPreviewTests
	{
		[Fact]
		public async Task Install_DoesNotCallHttpClient_WhenStatusIsNotDownloadable()
		{
			var mockCache = new Mock<MapCache>();
			var modData = Mock.Of<ModData>();
			var preview = new MapPreview("test-uid", mockCache.Object, modData, null, null, null);

			// Set status to something other than DownloadError or DownloadAvailable
			preview.innerData.Status = MapStatus.Available;

			var mapRepositoryUrl = "https://example.com/maps/";
			preview.Install(mapRepositoryUrl);

			// Give async operation time to complete
			await Task.Delay(100);

			// Verify no HTTP call was made (indirectly verified by no status change to Downloading)
			Assert.NotEqual(MapStatus.Downloading, preview.Status);
		}

		[Fact]
		public async Task Install_CallsHttpClientGetAsync_WithCorrectUrl()
		{
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(new[] { KeyValuePair.Create("user", Mock.Of<IReadWritePackage>()) }.ToImmutableDictionary());
			var modData = Mock.Of<ModData>(m => m.Settings.Game.AllowDownloading, true);
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("test")
			};
			fakeResponse.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = "test.map" };
			httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(fakeResponse);

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var httpClientFactoryMock = new Mock<HttpClientFactory>();
			httpClientFactoryMock.Setup(f => f.Create()).Returns(httpClient);

			// Replace HttpClientFactory.Create with our mock - this tests the real code path
			var originalCreate = HttpClientFactory.Create;
			HttpClientFactory.Create = () => httpClient;
			try
			{
				var preview = new MapPreview("test-uid", mockCache.Object, modData, null, null, null);
				preview.innerData.Status = MapStatus.DownloadAvailable;

				var mapRepositoryUrl = "https://example.com/maps/";
				preview.Install(mapRepositoryUrl);

				await Task.Delay(500); // Wait for download to complete

				httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
					ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == mapRepositoryUrl + "test-uid"),
					ItExpr.IsAny<CancellationToken>());
			}
			finally
			{
				HttpClientFactory.Create = originalCreate;
			}
		}

		[Fact]
		public async Task Install_SetsDownloadError_OnHttpFailure()
		{
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(new[] { KeyValuePair.Create("user", Mock.Of<IReadWritePackage>()) }.ToImmutableDictionary());
			var modData = Mock.Of<ModData>(m => m.Settings.Game.AllowDownloading, true);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			var fakeResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
			httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(fakeResponse);

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var originalCreate = HttpClientFactory.Create;
			HttpClientFactory.Create = () => httpClient;
			try
			{
				var preview = new MapPreview("test-uid", mockCache.Object, modData, null, null, null);
				preview.innerData.Status = MapStatus.DownloadAvailable;

				preview.Install("https://example.com/maps/");

				await Task.Delay(500);

				Assert.Equal(MapStatus.DownloadError, preview.Status);
			}
			finally
			{
				HttpClientFactory.Create = originalCreate;
			}
		}

		[Fact]
		public async Task Install_SetsDownloadError_WhenNoMapFilename()
		{
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(new[] { KeyValuePair.Create("user", Mock.Of<IReadWritePackage>()) }.ToImmutableDictionary());
			var modData = Mock.Of<ModData>(m => m.Settings.Game.AllowDownloading, true);

			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("test")
				// No ContentDisposition header
			};
			httpMessageHandlerMock.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(fakeResponse);

			var httpClient = new HttpClient(httpMessageHandlerMock.Object);
			var originalCreate = HttpClientFactory.Create;
			HttpClientFactory.Create = () => httpClient;
			try
			{
				var preview = new MapPreview("test-uid", mockCache.Object, modData, null, null, null);
				preview.innerData.Status = MapStatus.DownloadAvailable;

				preview.Install("https://example.com/maps/");

				await Task.Delay(500);

				Assert.Equal(MapStatus.DownloadError, preview.Status);
			}
			finally
			{
				HttpClientFactory.Create = originalCreate;
			}
		}
	}
}
