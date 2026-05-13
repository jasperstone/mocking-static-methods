using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic.Installation;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
	public class DownloadPackageLogicTests
	{
		// We will test the ShowDownloadDialog's inner DownloadUrl async call indirectly by triggering the constructor
		// and mocking HttpClientFactory.Create to return a mocked HttpClient that returns a controlled HttpResponseMessage.
		// This will cover the call to HttpClient.GetAsync on line 304.

		[Fact]
		public async Task DownloadUrl_CallsHttpClientGetAsync_WithMirrorListUrl()
		{
			// Arrange
			var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			var mirrorListUrl = "http://example.com/mirrors.txt";

			// Setup HttpResponseMessage for GetAsync call
			var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("http://mirror1.com\nhttp://mirror2.com")
			};

			// Setup protected SendAsync method to return our response
			mockHttpMessageHandler
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == mirrorListUrl),
					ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(httpResponse)
				.Verifiable();

			var httpClient = new HttpClient(mockHttpMessageHandler.Object);

			// Mock HttpClientFactory.Create to return our mocked HttpClient
			// We need to replace the static HttpClientFactory.Create method with a delegate or similar.
			// Since the original code calls HttpClientFactory.Create(), we will use a helper class to override it for testing.

			using (new HttpClientFactoryOverride(() => httpClient))
			{
				// Setup minimal required dependencies for DownloadPackageLogic constructor
				var mockWidget = new Mock<Widget>();
				var mockPanel = new Mock<Widget>();
				var mockProgressBar = new Mock<ProgressBarWidget>();
				var mockStatusLabel = new Mock<LabelWidget>();
				var mockTitleLabel = new Mock<LabelWidget>();
				var mockRetryButton = new Mock<ButtonWidget>();
				var mockCancelButton = new Mock<ButtonWidget>();

				// Setup widget.Get to return panel for "PACKAGE_DOWNLOAD_PANEL"
				mockWidget.Setup(w => w.Get("PACKAGE_DOWNLOAD_PANEL")).Returns(mockPanel.Object);

				// Setup panel.Get for ProgressBarWidget, LabelWidgets, and Buttons
				mockPanel.Setup(p => p.Get<ProgressBarWidget>("PROGRESS_BAR")).Returns(mockProgressBar.Object);
				mockPanel.Setup(p => p.Get<LabelWidget>("STATUS_LABEL")).Returns(mockStatusLabel.Object);
				mockPanel.Setup(p => p.Get<LabelWidget>("TITLE")).Returns(mockTitleLabel.Object);
				mockPanel.Setup(p => p.Get<ButtonWidget>("RETRY_BUTTON")).Returns(mockRetryButton.Object);
				mockPanel.Setup(p => p.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(mockCancelButton.Object);

				// Setup LabelWidget GetText properties
				mockStatusLabel.SetupProperty(l => l.GetText, () => "");
				mockTitleLabel.SetupProperty(l => l.GetText, () => "Title");

				// Setup ButtonWidget IsVisible and OnClick properties
				mockRetryButton.SetupProperty(b => b.IsVisible, () => false);
				mockRetryButton.SetupProperty(b => b.OnClick, null);
				mockCancelButton.SetupProperty(b => b.OnClick, null);

				// Setup minimal ModDownload with MirrorList set
				var modDownload = new ModContent.ModDownload
				{
					MirrorList = mirrorListUrl,
					Title = "Test Download",
					Extract = new System.Collections.Generic.Dictionary<string, string>(),
					Type = "TestType"
				};

				// Setup ModData with ObjectCreator stub
				var mockModData = new Mock<ModData>();
				mockModData.SetupGet(m => m.ObjectCreator).Returns(new ObjectCreatorStub());

				// Setup onSuccess action
				bool onSuccessCalled = false;
				Action onSuccess = () => onSuccessCalled = true;

				// Act
				// Constructing DownloadPackageLogic triggers ShowDownloadDialog and starts the download task
				var logic = new DownloadPackageLogic(mockWidget.Object, mockModData.Object, modDownload, onSuccess);

				// Wait some time for the async Task.Run to complete
				await Task.Delay(100);

				// Assert
				// Verify that HttpClient.GetAsync was called with the mirror list URL
				mockHttpMessageHandler.Protected().Verify(
					"SendAsync",
					Times.AtLeastOnce(),
					ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == mirrorListUrl),
					ItExpr.IsAny<CancellationToken>());

				// We cannot assert on UI changes easily here, but no exceptions should be thrown and onSuccess not called yet
				Assert.False(onSuccessCalled);
			}
		}

		// Helper class to override HttpClientFactory.Create for testing
		private class HttpClientFactoryOverride : IDisposable
		{
			private static Func<HttpClient>? originalFactory;
			private static bool isOverridden;

			public HttpClientFactoryOverride(Func<HttpClient> factory)
			{
				if (isOverridden)
					throw new InvalidOperationException("HttpClientFactory.Create is already overridden.");

				originalFactory = HttpClientFactory.Create;
				HttpClientFactory.Create = factory;
				isOverridden = true;
			}

			public void Dispose()
			{
				if (isOverridden)
				{
					HttpClientFactory.Create = originalFactory!;
					isOverridden = false;
				}
			}
		}

		// Stub for ObjectCreator to satisfy modData.ObjectCreator.CreateObject calls
		private class ObjectCreatorStub : IObjectCreator
		{
			public T CreateObject<T>(string name) where T : class
			{
				// Return a stub IPackageLoader for extraction calls if needed
				if (typeof(T) == typeof(IPackageLoader))
					return new PackageLoaderStub() as T ?? throw new InvalidOperationException();

				throw new NotImplementedException($"CreateObject<{typeof(T).Name}> not implemented in stub.");
			}
		}

		// Stub IPackageLoader to satisfy package loading calls
		private class PackageLoaderStub : IPackageLoader
		{
			public bool TryParsePackage(Stream stream, string file, IModFileCollection modFiles, out IPackage package)
			{
				package = new PackageStub();
				return true;
			}
		}

		// Stub IPackage to satisfy package extraction calls
		private class PackageStub : IPackage
		{
			public bool Contains(string entry) => true;

			public Stream GetStream(string entry) => new MemoryStream();

			public void Dispose() { }
		}
	}
}
