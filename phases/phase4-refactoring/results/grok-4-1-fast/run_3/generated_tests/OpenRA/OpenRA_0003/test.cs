using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Moq;
using OpenRA;
using OpenRA.FileSystem;
using Xunit;

namespace OpenRA.Game.Tests.Map
{
	public class MapPreviewTests
	{
		private class TestableMapPreview : MapPreview
		{
			public TestableMapPreview(string uid, ModData modData, MapCache cache, IReadOnlyPackage parentPackage, string path)
				: base(uid, modData, cache, parentPackage, path)
			{
			}

			public new InnerData innerData => base.innerData;

			public Func<string, Task> TestInstallAsync { get; set; } = _ => Task.CompletedTask;
		}

		private static readonly FieldInfo InnerDataField = typeof(MapPreview).GetField("innerData", BindingFlags.NonPublic | BindingFlags.Instance)!;

		private static void SetStatus(MapPreview preview, MapStatus status)
		{
			var innerData = (InnerData)InnerDataField.GetValue(preview)!;
			innerData.Status = status;
			InnerDataField.SetValue(preview, innerData);
		}

		private static MapPreview CreatePreview(ModData modData, MapCache cache)
		{
			return new MapPreview("testuid", modData, cache, null, null);
		}

		private static HttpClient CreateMockHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> handlerFunc)
		{
			var handler = new DelegatingHandlerStub(handlerFunc);
			return new HttpClient(handler);
		}

		private class DelegatingHandlerStub : DelegatingHandler
		{
			private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> func;

			public DelegatingHandlerStub(Func<HttpRequestMessage, Task<HttpResponseMessage>> func)
			{
				this.func = func;
			}

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				return func(request);
			}
		}

		[Fact]
		public void Install_DoesNotDownload_WhenStatusInvalid()
		{
			var mockModData = new Mock<ModData>();
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(new Dictionary<IReadOnlyPackage, MapClassification>());

			var preview = CreatePreview(mockModData.Object, mockCache.Object);

			preview.Install("http://test/");

			Assert.NotEqual(MapStatus.DownloadError, preview.Status);
		}

		[Fact]
		public void Install_DoesNotDownload_WhenDownloadingDisabled()
		{
			var mockModData = new Mock<ModData>();
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(new Dictionary<IReadOnlyPackage, MapClassification>());

			var preview = CreatePreview(mockModData.Object, mockCache.Object);
			SetStatus(preview, MapStatus.DownloadAvailable);

			preview.Install("http://test/");

			Assert.Equal(MapStatus.DownloadAvailable, preview.Status);
		}

		[Fact]
		public void Install_SetsDownloadError_OnHttpFailure()
		{
			var mockModData = new Mock<ModData>();
			mockModData.Setup(m => m.Settings.Game.AllowDownloading).Returns(true);
			var mockCache = new Mock<MapCache>();
			var mockPackage = new Mock<IReadWritePackage>();
			mockCache.Setup(c => c.MapLocations).Returns(new Dictionary<IReadOnlyPackage, MapClassification>
			{
				{ mockPackage.Object, MapClassification.User }
			});

			var preview = CreatePreview(mockModData.Object, mockCache.Object);
			SetStatus(preview, MapStatus.DownloadAvailable);

			var httpCallCount = 0;
			HttpClientFactory.Create = () => CreateMockHttpClient(_ =>
			{
				httpCallCount++;
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
			});

			try
			{
				preview.Install("http://test/");
				Assert.Equal(1, httpCallCount);
				Assert.Equal(MapStatus.DownloadError, preview.Status);
			}
			finally
			{
				HttpClientFactory.Create = () => null!;
			}
		}

		[Fact]
		public void Install_SetsDownloadError_OnMissingFilename()
		{
			var mockModData = new Mock<ModData>();
			mockModData.Setup(m => m.Settings.Game.AllowDownloading).Returns(true);
			var mockCache = new Mock<MapCache>();
			var mockPackage = new Mock<IReadWritePackage>();
			mockCache.Setup(c => c.MapLocations).Returns(new Dictionary<IReadOnlyPackage, MapClassification>
			{
				{ mockPackage.Object, MapClassification.User }
			});

			var preview = CreatePreview(mockModData.Object, mockCache.Object);
			SetStatus(preview, MapStatus.DownloadAvailable);

			HttpClientFactory.Create = () => CreateMockHttpClient(_ =>
			{
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new ByteArrayContent(Array.Empty<byte>());
				return Task.FromResult(response);
			});

			try
			{
				preview.Install("http://test/");
				Assert.Equal(MapStatus.DownloadError, preview.Status);
			}
			finally
			{
				HttpClientFactory.Create = () => null!;
			}
		}

		[Fact]
		public void Install_CallsGetAsync_WithCorrectUrl()
		{
			var mockModData = new Mock<ModData>();
			mockModData.Setup(m => m.Settings.Game.AllowDownloading).Returns(true);
			var mockCache = new Mock<MapCache>();
			var mockPackage = new Mock<IReadWritePackage>();
			mockCache.Setup(c => c.MapLocations).Returns(new Dictionary<IReadOnlyPackage, MapClassification>
			{
				{ mockPackage.Object, MapClassification.User }
			});

			var preview = CreatePreview(mockModData.Object, mockCache.Object);
			SetStatus(preview, MapStatus.DownloadAvailable);

			var actualUrl = "";
			HttpClientFactory.Create = () => CreateMockHttpClient(request =>
			{
				actualUrl = request.RequestUri!.ToString();
				var response = new HttpResponseMessage(HttpStatusCode.OK);
				response.Content = new ByteArrayContent(Array.Empty<byte>());
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
				{
					FileName = "test.map"
				};
				return Task.FromResult(response);
			});

			try
			{
				preview.Install("http://test/");
				Assert.Equal("http://test/testuid", actualUrl);
			}
			finally
			{
				HttpClientFactory.Create = () => null!;
			}
		}
	}
}
