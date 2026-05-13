using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;

public class DownloadPackageLogicTests
{
    [Fact]
    public async Task DownloadAsync_CallsGetAsync_WithCorrectUrl()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("response content")
        };
        mockHttpMessageHandler
            .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory
            .Setup(f => f.Create())
            .Returns(httpClient);

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

        var logic = new DownloadPackageLogic(widget, modData, download, onSuccess);
        // Inject the mocked HttpClientFactory
        typeof(DownloadPackageLogic)
            .GetField("HttpClientFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, mockHttpClientFactory.Object);

        // Act
        await logic.DownloadAsync("http://example.com/file");

        // Assert
        mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req => req.RequestUri.ToString() == "http://example.com/file")), Times.Once);
    }
}
