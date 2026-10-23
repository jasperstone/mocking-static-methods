using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
		private static MapPreview CreateMapPreview(ModData modData, MapCache cache, MapStatus status)
		{
			// Create with default constructor parameters via mocks
			var mapPreview = new MapPreview("test-uid", modData, cache, MapGridType.Invalid);
			
			// Set status via reflection (innerData is private/volatile)
			var innerDataField = typeof(MapPreview).GetField("innerData", 
				BindingFlags.NonPublic | BindingFlags.Instance);
			var innerData = innerDataField.GetValue(mapPreview);
			typeof(dynamic).GetProperty("Status").SetValue(innerData, status);
			
			return mapPreview;
		}

		[Fact]
		public void Install_DoesNotDownload_WhenStatusInvalid()
		{
			var mockCache = new Mock<MapCache>();
			var mockModData = new Mock<ModData>();
			mockCache.Setup(c => c.MapLocations).Returns(
				new Dictionary<IReadOnlyPackage, MapClassification>());

			var mapPreview = CreateMapPreview(mockModData.Object, mockCache.Object, MapStatus.Available);

			mapPreview.Install("http://test/");

			Assert.Equal(MapStatus.Available, mapPreview.Status);
		}

		[Fact]
		public void Install_SetsDownloadError_WhenNoUserInstallLocation()
		{
			var mockCache = new Mock<MapCache>();
			mockCache.Setup(c => c.MapLocations).Returns(
				new Dictionary<IReadOnlyPackage, MapClassification>());

			var mockModData = new Mock<ModData>();
			var mapPreview = CreateMapPreview(mockModData.Object, mockCache.Object, MapStatus.DownloadAvailable);

			mapPreview.Install("http://test/");

			Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
		}

		[Fact]
		public void Install_CallsHttpClientGetAsync_WithCorrectUrl()
		{
			// Arrange HTTP mock
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("test map data")
				});

			var mockClient = new HttpClient(mockHandler.Object);

			// Replace HttpClientFactory.Create using reflection
			var factoryType = Type.GetType("OpenRA.Extensibility.HttpClientFactory, OpenRA.Game");
			if (factoryType == null)
				throw new InvalidOperationException("HttpClientFactory type not found");

			var createField = factoryType.GetField("create", 
				BindingFlags.NonPublic | BindingFlags.Static);
			var originalCreate = createField.GetValue(null) as Func<HttpClient>;
			createField.SetValue(null, () => mockClient);

			try
			{
				var mockCache = new Mock<MapCache>();
				var mockPackage = new Mock<IReadWritePackage>();
				var mapLocations = new Dictionary<IReadOnlyPackage, MapClassification>
				{
					[mockPackage.Object] = MapClassification.User
				};
				mockCache.Setup(c => c.MapLocations).Returns(mapLocations);

				var mockModData = new Mock<ModData>();
				var mapPreview = CreateMapPreview(mockModData.Object, mockCache.Object, MapStatus.DownloadAvailable);

				// Act
				mapPreview.Install("http://test-repo/");

				// Assert - verifies line 672 GetAsync was called with correct URL
				mockHandler.Protected()
					.Verify("SendAsync", Times.Once(),
						ItExpr.Is<HttpRequestMessage>(req =>
							req.RequestUri.ToString() == "http://test-repo/test-uid" &&
							req.Method == HttpMethod.Get),
						ItExpr.IsAny<CancellationToken>());
			}
			finally
			{
				createField.SetValue(null, originalCreate);
			}
		}

		[Fact]
		public void Install_SetsDownloadError_OnHttpFailure()
		{
			// Arrange HTTP mock - 404 response
			var mockHandler = new Mock<HttpMessageHandler>();
			mockHandler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

			var mockClient = new HttpClient(mockHandler.Object);

			// Replace HttpClientFactory.Create using reflection
			var factoryType = Type.GetType("OpenRA.Extensibility.HttpClientFactory, OpenRA.Game");
			var createField = factoryType.GetField("create", 
				BindingFlags.NonPublic | BindingFlags.Static);
			var originalCreate = createField.GetValue(null) as Func<HttpClient>;
			createField.SetValue(null, () => mockClient);

			try
			{
				var mockCache = new Mock<MapCache>();
				var mockPackage = new Mock<IReadWritePackage>();
				var mapLocations = new Dictionary<IReadOnlyPackage, MapClassification>
				{
					[mockPackage.Object] = MapClassification.User
				};
				mockCache.Setup(c => c.MapLocations).Returns(mapLocations);

				var mockModData = new Mock<ModData>();
				var mapPreview = CreateMapPreview(mockModData.Object, mockCache.Object, MapStatus.DownloadAvailable);

				// Act
				mapPreview.Install("http://test/");

				// Assert
				Assert.Equal(MapStatus.DownloadError, mapPreview.Status);
			}
			finally
			{
				createField.SetValue(null, originalCreate);
			}
		}
	}
}
