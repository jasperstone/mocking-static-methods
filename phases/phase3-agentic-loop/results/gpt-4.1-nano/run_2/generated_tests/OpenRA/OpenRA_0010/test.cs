using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;

namespace OpenRA.Test
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadAsync_CallsGetAsyncAndHandlesResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("response content")
            };
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req => responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.Create()).Returns(httpClient);

            var widget = new Mock<Widget>().Object;
            var modData = new Mock<ModData>().Object;
            var download = new ModContent.ModDownload
            {
                URL = "http://example.com/file",
                SHA1 = null,
                Type = "TestType",
                Extract = new System.Collections.Generic.Dictionary<string, string>()
            };
            var onSuccess = new Action(() => { });

            // Instantiate the logic with a custom factory
            var logic = new DownloadPackageLogicWithFactory(widget, modData, download, onSuccess, mockFactory.Object);

            // Act
            await logic.DownloadAsync();

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.AtLeastOnce);
        }
    }

    // Extending the original class to inject the factory for testing
    public class DownloadPackageLogicWithFactory : DownloadPackageLogic
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DownloadPackageLogicWithFactory(Widget widget, ModData modData, ModContent.ModDownload download, Action onSuccess, IHttpClientFactory factory)
            : base(widget, modData, download, onSuccess)
        {
            _httpClientFactory = factory;
        }

        protected override HttpClient CreateHttpClient()
        {
            return _httpClientFactory.Create();
        }
    }
}
