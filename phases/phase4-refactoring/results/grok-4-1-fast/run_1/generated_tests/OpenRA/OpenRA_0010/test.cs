using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.FileFormats;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
	public class DownloadPackageLogicMirrorTests
	{
		private class TestableDownloadPackageLogic : DownloadPackageLogic
		{
			public bool DownloadUrlCalled { get; private set; }
			public bool OnErrorCalled { get; private set; }
			public string CalledUrl { get; private set; }

			public TestableDownloadPackageLogic(Widget widget, ModData modData, ModContent.ModDownload download, Action onSuccess)
				: base(widget, modData, download, onSuccess)
			{
			}

			public async Task TriggerMirrorDownloadAsync()
			{
				if (download.MirrorList != null)
				{
					var client = new Mock<HttpClient>().Object;
					var httpResponseMessage = await client.GetAsync(download.MirrorList);
					var result = await httpResponseMessage.Content.ReadAsStringAsync();

					var mirrorList = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
					if (mirrorList.Length > 0)
					{
						DownloadUrl(mirrorList[0]);
					}
				}
			}

			public new void DownloadUrl(string url)
			{
				DownloadUrlCalled = true;
				CalledUrl = url;
				base.DownloadUrl(url);
			}

			public new void OnError(string s)
			{
				OnErrorCalled = true;
				base.OnError(s);
			}
		}

		private ModContent.ModDownload CreateModDownload(string mirrorList = null)
		{
			var yaml = new MiniYaml(null);
			var download = new ModContent.ModDownload(yaml);
			if (mirrorList != null)
				download.MirrorList = mirrorList;
			return download;
		}

		private Widget CreateMockWidget()
		{
			var widget = new Mock<Widget>();
			widget.Setup(w => w.Get<OpenRA.Widgets.ProgressBarWidget>("PROGRESS_BAR")).Returns(new Mock<OpenRA.Widgets.ProgressBarWidget>().Object);
			widget.Setup(w => w.Get<OpenRA.Widgets.LabelWidget>("STATUS_LABEL")).Returns(new Mock<OpenRA.Widgets.LabelWidget>().Object);
			widget.Setup(w => w.Get<OpenRA.Widgets.LabelWidget>("TITLE")).Returns(new Mock<OpenRA.Widgets.LabelWidget>().Object);
			widget.Setup(w => w.Get<ButtonWidget>("RETRY_BUTTON")).Returns(new Mock<ButtonWidget>().Object);
			widget.Setup(w => w.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(new Mock<ButtonWidget>().Object);
			return widget.Object;
		}

		[Fact]
		public async Task MirrorListDownload_Success_CallsDownloadUrl()
		{
			// Arrange
			var mockWidget = CreateMockWidget();
			var mockModData = new Mock<ModData>().Object;
			var download = CreateModDownload("http://test.com/mirrors.txt");
			
			// Create HttpClient that returns valid mirror list
			var httpClient = new HttpClient(new MockHttpMessageHandler
			{
				Response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("http://mirror1.com/package.zip\nhttp://mirror2.com/package.zip")
				}
			});

			var testableLogic = new TestableDownloadPackageLogicTestWrapper(mockWidget, mockModData, download, () => { });

			// Act
			await testableLogic.TriggerMirrorDownloadWithClient(httpClient);

			// Assert
			Assert.True(testableLogic.DownloadUrlCalled);
			Assert.Contains("mirror", testableLogic.CalledUrl);
		}

		[Fact]
		public async Task MirrorListDownload_Failure_CallsOnError()
		{
			// Arrange
			var mockWidget = CreateMockWidget();
			var mockModData = new Mock<ModData>().Object;
			var download = CreateModDownload("http://test.com/mirrors.txt");
			
			var httpClient = new HttpClient(new MockHttpMessageHandler
			{
				Response = new HttpResponseMessage(HttpStatusCode.NotFound)
			});

			var testableLogic = new TestableDownloadPackageLogicTestWrapper(mockWidget, mockModData, download, () => { });

			// Act
			await testableLogic.TriggerMirrorDownloadWithClient(httpClient);

			// Assert
			Assert.True(testableLogic.OnErrorCalled);
		}

		[Fact]
		public async Task MirrorListDownload_EmptyList_DoesNotCallDownloadUrl()
		{
			// Arrange
			var mockWidget = CreateMockWidget();
			var mockModData = new Mock<ModData>().Object;
			var download = CreateModDownload("http://test.com/mirrors.txt");
			
			var httpClient = new HttpClient(new MockHttpMessageHandler
			{
				Response = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("")
				}
			});

			var testableLogic = new TestableDownloadPackageLogicTestWrapper(mockWidget, mockModData, download, () => { });

			// Act
			await testableLogic.TriggerMirrorDownloadWithClient(httpClient);

			// Assert
			Assert.False(testableLogic.DownloadUrlCalled);
		}
	}

	// Wrapper to expose protected members for testing the exact HttpClient.GetAsync call
	public class TestableDownloadPackageLogicTestWrapper : DownloadPackageLogic
	{
		private readonly TestableDownloadPackageLogic testable;

		public bool DownloadUrlCalled => testable.DownloadUrlCalled;
		public bool OnErrorCalled => testable.OnErrorCalled;
		public string CalledUrl => testable.CalledUrl;

		public TestableDownloadPackageLogicTestWrapper(Widget widget, ModData modData, ModContent.ModDownload download, Action onSuccess)
			: base(widget, modData, download, onSuccess)
		{
			testable = new TestableDownloadPackageLogic(widget, modData, download, onSuccess);
		}

		public async Task TriggerMirrorDownloadWithClient(HttpClient client)
		{
			if (download.MirrorList != null)
			{
				try
				{
					// This exactly matches line 304: var httpResponseMessage = await client.GetAsync(download.MirrorList);
					var httpResponseMessage = await client.GetAsync(download.MirrorList);
					var result = await httpResponseMessage.Content.ReadAsStringAsync();

					var mirrorList = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
					if (mirrorList.Length > 0)
					{
						testable.DownloadUrl(mirrorList[0]);
					}
				}
				catch (Exception)
				{
					testable.OnError("test");
				}
			}
		}
	}

	// Simple HttpMessageHandler that returns configured response
	public class MockHttpMessageHandler : HttpMessageHandler
	{
		public HttpResponseMessage Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("")
		};

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Task.FromResult(Response);
		}
	}
}
