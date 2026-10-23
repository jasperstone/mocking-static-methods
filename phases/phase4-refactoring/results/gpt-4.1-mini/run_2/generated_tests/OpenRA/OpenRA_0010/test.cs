using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
	public class DownloadPackageLogicTests
	{
		// Since we cannot override or mock HttpClientFactory.Create or DownloadUrl,
		// we test that the DownloadPackageLogic constructor triggers the mirror list download logic
		// by verifying that the DownloadUrl method is called with the expected URL.
		// We do this by subclassing DownloadPackageLogic and hiding DownloadUrl with new method.

		class TestDownloadPackageLogic : DownloadPackageLogic
		{
			public string DownloadedUrl { get; private set; }
			public bool DownloadUrlCalled { get; private set; }

			public TestDownloadPackageLogic(
				Widget widget,
				object modData,
				object download,
				Action onSuccess)
				: base(widget, modData, download, onSuccess)
			{
			}

			// Hide DownloadUrl method to capture the URL instead of performing download
			protected new void DownloadUrl(string url)
			{
				DownloadUrlCalled = true;
				DownloadedUrl = url;
			}
		}

		[Fact]
		public void Constructor_WithMirrorList_TriggersDownloadUrl()
		{
			// Arrange
			var widgetMock = new Mock<Widget>();
			var panelMock = new Mock<Widget>();
			var progressBarMock = new Mock<ProgressBarWidget>();

			widgetMock.Setup(w => w.Get("PACKAGE_DOWNLOAD_PANEL")).Returns(panelMock.Object);
			panelMock.Setup(p => p.Get<ProgressBarWidget>("PROGRESS_BAR")).Returns(progressBarMock.Object);

			var labelWidgetMock = new Mock<LabelWidget>();
			panelMock.Setup(p => p.Get<LabelWidget>("STATUS_LABEL")).Returns(labelWidgetMock.Object);
			panelMock.Setup(p => p.Get<LabelWidget>("TITLE")).Returns(labelWidgetMock.Object);
			panelMock.Setup(p => p.Get<ButtonWidget>("RETRY_BUTTON")).Returns(new ButtonWidget());
			panelMock.Setup(p => p.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(new ButtonWidget());

			// Create a dynamic object for download with MirrorList property
			var download = new
			{
				MirrorList = "http://example.com/mirrors.txt",
				URL = "http://example.com/download.zip",
				Title = "Test Download",
				Type = "TestType",
				Extract = new Dictionary<string, string>(),
				SHA1 = (string)null
			};

			// ModData can be null for this test as we don't reach extraction
			object modData = null;

			bool onSuccessCalled = false;

			// Act
			var logic = new TestDownloadPackageLogic(widgetMock.Object, modData, download, () => onSuccessCalled = true);

			// Assert
			Assert.True(logic.DownloadUrlCalled);
			Assert.Equal(download.MirrorList, logic.DownloadedUrl);
		}
	}
}
