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

namespace OpenRA.Tests.Mods.Common.Widgets.Logic
{
	public class DownloadPackageLogicTests
	{
		[Fact]
		public async Task ShowDownloadDialog_WithMirrorList_CallsHttpClientGetAsync()
		{
			// Arrange
			var mockWidget = new Mock<Widget>();
			var mockPanel = new Mock<Widget>();
			var mockProgressBar = new Mock<ProgressBarWidget>();
			var mockStatusLabel = new Mock<LabelWidget>();
			var mockTitleLabel = new Mock<LabelWidget>();
			var mockRetryButton = new Mock<ButtonWidget>();
			var mockCancelButton = new Mock<ButtonWidget>();

			// Setup widget.Get calls
			mockWidget.Setup(w => w.Get("PACKAGE_DOWNLOAD_PANEL")).Returns(mockPanel.Object);
			mockPanel.Setup(p => p.Get<ProgressBarWidget>("PROGRESS_BAR")).Returns(mockProgressBar.Object);
			mockPanel.Setup(p => p.Get<LabelWidget>("STATUS_LABEL")).Returns(mockStatusLabel.Object);
			mockPanel.Setup(p => p.Get<LabelWidget>("TITLE")).Returns(mockTitleLabel.Object);
			mockPanel.Setup(p => p.Get<ButtonWidget>("RETRY_BUTTON")).Returns(mockRetryButton.Object);
			mockPanel.Setup(p => p.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(mockCancelButton.Object);

			// Setup LabelWidget Font and Bounds for TruncateText call
			mockStatusLabel.SetupGet(l => l.Font).Returns("default");
			mockStatusLabel.SetupGet(l => l.Bounds).Returns(new Rectangle(100, 20, 200, 30));

			// Setup Game.Renderer.Fonts to avoid null reference
			Game.Renderer.Fonts["default"] = new Font();

			// Setup FluentProvider.GetMessage to return simple strings
			FluentProvider.SetMessage("label-downloading", "Downloading");
			FluentProvider.SetMessage("label-fetching-mirror-list", "Fetching mirror list");
			FluentProvider.SetMessage("label-unknown-host", "Unknown host");
			FluentProvider.SetMessage("label-download-failed", "Download failed");
			FluentProvider.SetMessage("label-mirror-selection-failed", "Mirror selection failed");

			// Setup ModDownload with MirrorList
			var modDownload = new ModContent.ModDownload
			{
				MirrorList = "http://example.com/mirrors.txt",
				Title = "Test Download",
				Type = "TestType",
				Extract = new System.Collections.Generic.Dictionary<string, string>(),
				SHA1 = null
			};

			// Setup ModData and ObjectCreator
			var mockObjectCreator = new Mock<ObjectCreator>();
			var modData = new ModData { ObjectCreator = mockObjectCreator.Object };

			// Setup HttpClientFactory to return a mocked HttpClient
			var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
			var mockHttpResponse = new Mock<HttpResponseMessage>();
			var mockHttpContent = new Mock<HttpContent>();

			// Setup HttpResponseMessage and Content
			mockHttpResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.OK);
			mockHttpResponse.SetupGet(r => r.Content).Returns(mockHttpContent.Object);
			mockHttpContent.Setup(c => c.ReadAsStringAsync()).ReturnsAsync("http://mirror1\nhttp://mirror2");

			// Setup HttpClient.GetAsync to return the mocked response
			mockHttpClient
				.Setup(c => c.GetAsync(It.IsAny<string>(), HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()))
				.ReturnsAsync(mockHttpResponse.Object);

			// Setup HttpClientFactory.Create to return the mocked HttpClient
			HttpClientFactory.SetCreateFunc(() => mockHttpClient.Object);

			// Setup DownloadUrl method to be testable by invoking ShowDownloadDialog
			Action onSuccess = () => { };

			// Act
			var logic = new DownloadPackageLogic(mockWidget.Object, modData, modDownload, onSuccess);

			// Wait briefly to allow async Task.Run to start
			await Task.Delay(100);

			// Assert
			mockHttpClient.Verify(c => c.GetAsync("http://example.com/mirrors.txt", HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.AtLeastOnce);

			// Cleanup
			HttpClientFactory.ResetCreateFunc();
		}
	}
}
