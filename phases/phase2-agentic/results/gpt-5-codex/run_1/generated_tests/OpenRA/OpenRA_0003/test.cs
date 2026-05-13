using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA;
using OpenRA.FileFormats;
using OpenRA.FileSystem;
using OpenRA.Graphics;
using OpenRA.Mods.Common;
using Xunit;

namespace OpenRA.Tests.Map
{
	public class MapPreviewInstallTests
	{
		[Fact]
		public void Install_SetsDownloadError_WhenHttpClientResponseNotSuccess()
		{
			// Arrange
			var (preview, installPackage) = MapPreviewTestHelper.CreateMapPreview(MapStatus.DownloadAvailable, true, HttpStatusCode.NotFound, null);

			// Act
			preview.Install("http://example.com/maps/");

			// Wait for background task
			MapPreviewTestHelper.WaitForDownloadCompletion(preview);

			// Assert
			Assert.Equal(MapStatus.DownloadError, preview.Status);
			installPackage.Verify(c => c.Update(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
		}

		[Fact]
		public void Install_SetsDownloadError_WhenContentDispositionMissing()
		{
			// Arrange
			var (preview, installPackage) = MapPreviewTestHelper.CreateMapPreview(MapStatus.DownloadAvailable, true, HttpStatusCode.OK, null);

			// Act
			preview.Install("http://example.com/maps/");

			// Wait for background task
			MapPreviewTestHelper.WaitForDownloadCompletion(preview);

			// Assert
			Assert.Equal(MapStatus.DownloadError, preview.Status);
			installPackage.Verify(c => c.Update(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
		}

		[Fact]
		public void Install_DownloadsAndUpdatesPackage_WhenResponseIsValid()
		{
			// Arrange
			var (preview, installPackage) = MapPreviewTestHelper.CreateMapPreview(MapStatus.DownloadAvailable, true, HttpStatusCode.OK, "downloaded.map");
			var updateCalled = false;
			byte[] updatedBytes = null;
			string updatedFilename = null;

			installPackage
				.Setup(c => c.Update(It.IsAny<string>(), It.IsAny<byte[]>()))
				.Callback<string, byte[]>((filename, data) =>
				{
					updateCalled = true;
					updatedFilename = filename;
					updatedBytes = data;
				});

			installPackage.Setup(c => c.OpenPackage("downloaded.map", It.IsAny<IReadOnlyFileSystem>())).Returns(Mock.Of<IReadOnlyPackage>());

			// Act
			preview.Install("http://example.com/maps/");

			// Wait for background task
			MapPreviewTestHelper.WaitForDownloadCompletion(preview);

			// Assert
			Assert.True(updateCalled);
			Assert.Equal("downloaded.map", updatedFilename);
			Assert.NotNull(updatedBytes);
			Assert.Equal(MapClassification.User, preview.Class);
			Assert.NotEqual(MapStatus.DownloadError, preview.Status);
		}
	}

	internal static class MapPreviewTestHelper
	{
		static readonly PropertyInfo ClientFactoryProperty =
			typeof(HttpClientFactory).GetProperty("Factory", BindingFlags.Static | BindingFlags.NonPublic);

		public static (MapPreview preview, Mock<IReadWritePackage> installPackage) CreateMapPreview(MapStatus initialStatus, bool allowDownloading, HttpStatusCode responseStatus, string fileName)
		{
			var modData = TestModDataFactory.Create();
			var cache = new MapCache(modData.ModFiles, "test");
			var innerPreview = cache.RemoteMaps.First();

			var installPackage = new Mock<IReadWritePackage>();
			var mapLocations = new[]
			{
				new KeyValuePair<IReadOnlyPackage, MapClassification>(installPackage.Object, MapClassification.User)
			};

			var mockCache = new Mock<MapCache>(modData.ModFiles, "test") { CallBase = true };
			mockCache.Setup(c => c.MapLocations).Returns(mapLocations.ToImmutableDictionary());

			// Inject HttpClient mock
			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith("http://example.com/maps/")), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(() =>
				{
					var response = new HttpResponseMessage(responseStatus)
					{
						Content = new ByteArrayContent(responseStatus == HttpStatusCode.OK ? new byte[256] : Array.Empty<byte>())
					};

					if (responseStatus == HttpStatusCode.OK && fileName != null)
					{
						response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
						{
							FileName = fileName
						};
					}

					return response;
				});

			var httpClient = new HttpClient(handlerMock.Object);
			ClientFactoryProperty.SetValue(null, new Func<HttpClient>(() => httpClient));

			var preview = new MapPreview(innerPreview.Uid, mockCache.Object, modData);
			SetStatus(preview, initialStatus);
			SetAllowDownloading(modData, allowDownloading);

			return (preview, installPackage);
		}

		public static void WaitForDownloadCompletion(MapPreview preview)
		{
			for (var attempt = 0; attempt < 50; attempt++)
			{
				if (preview.Status != MapStatus.Downloading)
					return;

				Thread.Sleep(100);
			}
		}

		static void SetStatus(MapPreview preview, MapStatus status)
		{
			var innerDataField = typeof(MapPreview).GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance);
			if (innerDataField?.GetValue(preview) is not null)
			{
				var innerDataType = innerDataField.FieldType;
				var cloned = innerDataField.GetValue(preview);
				var statusField = innerDataType.GetField("Status", BindingFlags.Public | BindingFlags.Instance);
				statusField?.SetValue(cloned, status);
			}
		}

		static void SetAllowDownloading(ModData modData, bool allowDownloading)
		{
			var settings = modData.Settings;
			settings.Game.AllowDownloading = allowDownloading;
		}
	}
}
