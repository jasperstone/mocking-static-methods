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

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
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

			// Setup widget hierarchy and returns
			mockWidget.Setup(w => w.Get("PACKAGE_DOWNLOAD_PANEL")).Returns(mockPanel.Object);
			mockPanel.Setup(p => p.Get<ProgressBarWidget>("PROGRESS_BAR")).Returns(mockProgressBar.Object);
			mockPanel.Setup(p => p.Get<LabelWidget>("STATUS_LABEL")).Returns(mockStatusLabel.Object);
			mockPanel.Setup(p => p.Get<LabelWidget>("TITLE")).Returns(mockTitleLabel.Object);
			mockPanel.Setup(p => p.Get<ButtonWidget>("RETRY_BUTTON")).Returns(mockRetryButton.Object);
			mockPanel.Setup(p => p.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(mockCancelButton.Object);

			// Setup fonts and bounds for status label
			var mockFont = new Mock<OpenRA.Renderer.Font>();
			mockFont.SetupGet(f => f.Name).Returns("TestFont");
			OpenRA.Game.Renderer.Fonts["TestFont"] = mockFont.Object;
			mockStatusLabel.SetupGet(l => l.Font).Returns("TestFont");
			mockStatusLabel.SetupGet(l => l.Bounds).Returns(new OpenRA.Primitives.Rect(0, 0, 100, 20));

			// Setup GetText to return empty string initially
			mockStatusLabel.SetupProperty(l => l.GetText, () => "");
			mockTitleLabel.SetupProperty(l => l.GetText, () => "");

			// Setup a dummy ModDownload with MirrorList set
			var download = new ModContent.ModDownload
			{
				MirrorList = "http://example.com/mirrors.txt",
				Title = "TestDownload",
				Type = "TestType",
				Extract = new System.Collections.Generic.Dictionary<string, string>(),
				SHA1 = null,
				URL = "http://example.com/download"
			};

			// Setup ModData with dummy ObjectCreator
			var modData = new ModData
			{
				ObjectCreator = new ObjectCreator()
			};

			// Setup HttpClientFactory to return a mocked HttpClient
			var mockHttpClient = new Mock<HttpClient>(MockBehavior.Strict);
			var mockHttpResponse = new Mock<HttpResponseMessage>();
			var mockHttpContent = new Mock<HttpContent>();

			// Setup HttpResponseMessage and Content
			mockHttpResponse.SetupGet(r => r.StatusCode).Returns(HttpStatusCode.OK);
			mockHttpResponse.SetupGet(r => r.Content).Returns(mockHttpContent.Object);
			mockHttpContent.Setup(c => c.ReadAsStringAsync()).ReturnsAsync("http://mirror1\nhttp://mirror2");

			// Setup HttpClient GetAsync to return the mocked response
			mockHttpClient
				.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(mockHttpResponse.Object)
				.Verifiable();

			// Replace HttpClientFactory.Create to return our mockHttpClient
			OpenRA.Mods.Common.Widgets.Logic.HttpClientFactory.Create = () => mockHttpClient.Object;

			// Act
			var logic = new DownloadPackageLogic(mockWidget.Object, modData, download, () => { });

			// Wait a bit for the async Task.Run to execute
			await Task.Delay(100);

			// Assert
			mockHttpClient.Verify(c => c.GetAsync(download.MirrorList, HttpCompletionOption.ResponseHeadersRead, It.IsAny<CancellationToken>()), Times.AtLeastOnce());
		}
	}
}
