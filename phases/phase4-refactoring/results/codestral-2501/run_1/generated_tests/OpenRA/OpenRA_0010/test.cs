using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

public class DownloadPackageLogicTests
{
    [Fact]
    public async Task GetAsync_ShouldBeCalled_WhenFetchingMirrors()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        var mockModContent = new Mock<ModContent>();
        var mockModDownload = new Mock<ModContent.ModDownload>(new MiniYaml(""));

        mockModDownload.SetupGet(d => d.MirrorList).Returns("http://example.com/mirrors");

        var downloadPackageLogic = new DownloadPackageLogic(null, mockModContent.Object, mockModDownload.Object, () => { });

        mockHttpMessageHandler
            .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpResponseMessage
            {
                Content = new StringContent("mirror1\nmirror2")
            });

        // Act
        await Task.Delay(1000); // Wait for the async task to complete

        // Assert
        mockHttpMessageHandler.Verify(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
