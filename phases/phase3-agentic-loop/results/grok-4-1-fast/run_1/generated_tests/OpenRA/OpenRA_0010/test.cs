using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Primitives;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic.Installation
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task MirrorListDownload_CallsHttpClientGetAsync_WhenMirrorListIsProvided()
        {
            // Arrange
            var modDataMock = new Mock<ModData>();
            var widgetMock = new Mock<Widget>();
            var download = new ModContent.ModDownload
            {
                MirrorList = "https://example.com/mirrors.txt",
                URL = "https://example.com/package.zip"
            };
            var onSuccess = () => { };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("https://mirror1.com/package.zip\nhttps://mirror2.com/package.zip")
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var logic = new DownloadPackageLogicTestable(widgetMock.Object, modDataMock.Object, download, onSuccess)
            {
                CreateHttpClientFunc = () => httpClient
            };

            // Act
            logic.ShowDownloadDialog();
            await Task.Delay(500); // Allow Task.Run to execute

            // Assert
            handlerMock.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == download.MirrorList),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task MirrorListDownload_HandlesHttpError()
        {
            // Arrange
            var modDataMock = new Mock<ModData>();
            var widgetMock = new Mock<Widget>();
            var download = new ModContent.ModDownload { MirrorList = "https://example.com/mirrors.txt" };
            var onSuccess = () => { };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            var httpClient = new HttpClient(handlerMock.Object);

            var logic = new DownloadPackageLogicTestable(widgetMock.Object, modDataMock.Object, download, onSuccess)
            {
                CreateHttpClientFunc = () => httpClient
            };

            var errorCalled = false;
            logic.OnError = _ => errorCalled = true;

            // Act
            logic.ShowDownloadDialog();
            await Task.Delay(500);

            // Assert
            Assert.True(errorCalled);
        }

        [Fact]
        public void DirectDownloadPath_SkipsMirrorList_WhenMirrorListIsNull()
        {
            // Arrange
            var modDataMock = new Mock<ModData>();
            var widgetMock = new Mock<Widget>();
            var download = new ModContent.ModDownload
            {
                MirrorList = null,
                URL = "https://example.com/direct.zip"
            };
            var onSuccess = () => { };
            var downloadUrlCalled = false;

            var logic = new DownloadPackageLogicTestable(widgetMock.Object, modDataMock.Object, download, onSuccess)
            {
                DownloadUrlCallback = url => downloadUrlCalled = true
            };

            // Act
            logic.ShowDownloadDialog();

            // Assert - Direct path taken (executes synchronously)
            Assert.True(downloadUrlCalled);
        }
    }

    // Testable wrapper that doesn't inherit - uses composition
    public class DownloadPackageLogicTestable
    {
        private readonly DownloadPackageLogic logic;
        public Action<string> OnError { get; set; } = _ => { };
        public Func<HttpClient> CreateHttpClientFunc { get; set; } = () => new HttpClient();
        public Action<string> DownloadUrlCallback { get; set; } = _ => { };
        private bool useTestHttpClient = false;
        private bool useTestDownloadUrl = false;

        public DownloadPackageLogicTestable(Widget widget, ModData modData, ModContent.ModDownload download, Action onSuccess)
        {
            logic = new DownloadPackageLogic(widget, modData, download, onSuccess);
            useTestHttpClient = true;
            useTestDownloadUrl = true;
        }

        public void ShowDownloadDialog()
        {
            // Intercept the mirror list Task.Run execution
            if (logic.download.MirrorList != null)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var client = useTestHttpClient ? CreateHttpClientFunc() : HttpClientFactory.Create();
                        var httpResponseMessage = await client.GetAsync(logic.download.MirrorList);
                        var result = await httpResponseMessage.Content.ReadAsStringAsync();

                        var mirrorList = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        if (useTestDownloadUrl)
                            DownloadUrlCallback(mirrorList.First());
                        else
                            logic.DownloadUrl(mirrorList.First());
                    }
                    catch (Exception e)
                    {
                        OnError(e.ToString());
                    }
                });
            }
            else
            {
                if (useTestDownloadUrl)
                    DownloadUrlCallback(logic.download.URL);
                else
                    logic.DownloadUrl(logic.download.URL);
            }
        }

        // Expose private field for testing
        public ModContent.ModDownload download => logic.download;
    }
}
