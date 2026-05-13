using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Mods.Common;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class DownloadPackageLogicTests
	{
		[Fact]
		public async Task ShowDownloadDialog_UsesHttpClientGetAsync_WithMirrorList()
		{
			// Arrange
			var mockWidget = new Mock<Widget>();
			var mockPanel = new Mock<Widget>();
			var mockProgressBar = new Mock<ProgressBarWidget>();
			var mockStatusLabel = new Mock<LabelWidget>();
			var mockTitleLabel = new Mock<LabelWidget>();
			var mockRetryButton = new Mock<ButtonWidget>();
			var mockCancelButton = new Mock<ButtonWidget>();

			// Setup widget.Get calls for panel and its children
			mockWidget.Setup(w => w.Get("PACKAGE_DOWNLOAD_PANEL")).Returns(mockPanel.Object);
			mockPanel.Setup(p => p.Get<ProgressBarWidget>("PROGRESS_BAR")).Returns(mockProgressBar.Object);
			mockPanel.Setup(p => p.Get<LabelWidget>("STATUS_LABEL")).Returns(mockStatusLabel.Object);
			mockPanel.Setup(p => p.Get<LabelWidget>("TITLE")).Returns(mockTitleLabel.Object);
			mockPanel.Setup(p => p.Get<ButtonWidget>("RETRY_BUTTON")).Returns(mockRetryButton.Object);
			mockPanel.Setup(p => p.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(mockCancelButton.Object);

			// Setup statusLabel.Font and Bounds for CachedTransform usage
			mockStatusLabel.SetupGet(l => l.Font).Returns("default");
			mockStatusLabel.SetupGet(l => l.Bounds).Returns(new Rectangle(0, 0, 100, 20));

			// Setup Game.Renderer.Fonts to avoid null reference (simulate)
			Game.Renderer = new MockRenderer();

			// Setup FluentProvider.GetMessage to return simple strings
			FluentProvider.SetMockMessageProvider((key, args) => key);

			// Setup a ModDownload with MirrorList set
			var modDownload = new ModContent.ModDownload
			{
				MirrorList = "http://example.com/mirrors.txt",
				Title = "Test Download",
				Type = "TestType",
				Extract = new System.Collections.Generic.Dictionary<string, string>(),
				SHA1 = null
			};

			// Setup ModData with ObjectCreator mock
			var mockObjectCreator = new Mock<ObjectCreator>();
			mockObjectCreator.Setup(oc => oc.CreateObject<IPackageLoader>(It.IsAny<string>())).Returns(Mock.Of<IPackageLoader>());
			var modData = new ModData { ObjectCreator = mockObjectCreator.Object };

			// Setup HttpClientFactory to return a mocked HttpClient
			var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
			var mockHttpResponse = new Mock<HttpResponseMessage>(MockBehavior.Strict);
			var mockHttpContent = new Mock<HttpContent>(MockBehavior.Strict);

			// Setup HttpClientFactory.Create to return our mockHttpClient
			HttpClientFactory.SetMockClientFactory(() => mockHttpClient.Object);

			// Setup HttpClient.GetAsync to return a successful response
			mockHttpClient
				.Setup(c => c.GetAsync(It.IsAny<string>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					var response = new HttpResponseMessage(HttpStatusCode.OK);
					response.Content = new StringContent("mirror1\nmirror2\nmirror3");
					return response;
				});

			// Act
			// Constructing DownloadPackageLogic triggers ShowDownloadDialog and starts the download task
			var logic = new DownloadPackageLogic(mockWidget.Object, modData, modDownload, () => { });

			// Wait a short time to allow the async Task.Run to execute
			await Task.Delay(100);

			// Assert
			// Verify that HttpClient.GetAsync was called with the MirrorList URL
			mockHttpClient.Verify(c => c.GetAsync(modDownload.MirrorList, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.AtLeastOnce());
		}
	}

	// Helper classes and mocks to support the test environment

	// Minimal Rectangle struct for Bounds property
	public struct Rectangle
	{
		public int X, Y, Width, Height;
		public Rectangle(int x, int y, int width, int height)
		{
			X = x; Y = y; Width = width; Height = height;
		}
	}

	// Mock Renderer with Fonts dictionary
	public class MockRenderer : IRenderer
	{
		public System.Collections.Generic.Dictionary<string, IFont> Fonts { get; } = new System.Collections.Generic.Dictionary<string, IFont>
		{
			{ "default", new MockFont() }
		};
	}

	// Mock Font
	public class MockFont : IFont
	{
	}

	// Mock FluentProvider to return keys as messages
	public static class FluentProvider
	{
		private static Func<string, object[], string> _messageProvider = (key, args) => key;

		public static string GetMessage(string key, params object[] args) => _messageProvider(key, args);

		public static void SetMockMessageProvider(Func<string, object[], string> provider)
		{
			_messageProvider = provider;
		}
	}

	// Mock HttpClientFactory to allow injection of HttpClient
	public static class HttpClientFactory
	{
		private static Func<HttpClient> _clientFactory = () => new HttpClient();

		public static HttpClient Create() => _clientFactory();

		public static void SetMockClientFactory(Func<HttpClient> factory)
		{
			_clientFactory = factory;
		}
	}

	// Minimal interfaces and classes to satisfy dependencies
	public interface IFont { }
	public interface IRenderer
	{
		System.Collections.Generic.Dictionary<string, IFont> Fonts { get; }
	}

	public static class Game
	{
		public static IRenderer Renderer { get; set; }

		public static void RunAfterTick(Action action)
		{
			action();
		}
	}

	public static class Ui
	{
		public static void CloseWindow()
		{
		}
	}

	public class ModData
	{
		public ObjectCreator ObjectCreator { get; set; }
		public ModFiles ModFiles { get; set; }
	}

	public class ModFiles { }

	public class ObjectCreator
	{
		public virtual T CreateObject<T>(string name) where T : class => null;
	}

	public interface IPackageLoader
	{
		bool TryParsePackage(Stream stream, string file, ModFiles modFiles, out IPackage package);
	}

	public interface IPackage : IDisposable
	{
		bool Contains(string entry);
		Stream GetStream(string entry);
	}

	public class ModContent
	{
		public class ModDownload
		{
			public string MirrorList { get; set; }
			public string URL { get; set; }
			public string Title { get; set; }
			public string Type { get; set; }
			public System.Collections.Generic.Dictionary<string, string> Extract { get; set; }
			public string SHA1 { get; set; }
		}
	}

	public class Widget
	{
		public virtual T Get<T>(string name) where T : class => null;
		public virtual Widget Get(string name) => null;
	}

	public class LabelWidget : Widget
	{
		public virtual string Font { get; set; }
		public virtual Rectangle Bounds { get; set; }
		public virtual Func<string> GetText { get; set; }
	}

	public class ProgressBarWidget : Widget
	{
		public virtual bool Indeterminate { get; set; }
		public virtual int Percentage { get; set; }
	}

	public class ButtonWidget : Widget
	{
		public virtual Func<bool> IsVisible { get; set; }
		public virtual Action OnClick { get; set; }
	}
}
