using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Widgets;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class DownloadPackageLogicTests
    {
        [Fact]
        public async Task DownloadUrl_ShouldCallGetAsyncWithCorrectUrl()
        {
            // Arrange
            var mockHttpClient = new Mock<HttpClient>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpClient.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Mocked content")
                });

            var mockModData = new Mock<ModData>();
            var mockModDownload = new Mock<ModContent.ModDownload>();
            var mockWidget = new Mock<Widget>();
            var mockProgressBar = new Mock<ProgressBarWidget>();
            var mockLabelWidget = new Mock<LabelWidget>();
            var mockButtonWidget = new Mock<ButtonWidget>();

            mockWidget.Setup(w => w.Get("PACKAGE_DOWNLOAD_PANEL")).Returns(mockWidget.Object);
            mockWidget.Setup(w => w.Get<ProgressBarWidget>("PROGRESS_BAR")).Returns(mockProgressBar.Object);
            mockWidget.Setup(w => w.Get<LabelWidget>("STATUS_LABEL")).Returns(mockLabelWidget.Object);
            mockWidget.Setup(w => w.Get<ButtonWidget>("RETRY_BUTTON")).Returns(mockButtonWidget.Object);
            mockWidget.Setup(w => w.Get<ButtonWidget>("CANCEL_BUTTON")).Returns(mockButtonWidget.Object);

            var logic = new DownloadPackageLogic(mockWidget.Object, mockModData.Object, mockModDownload.Object, () => { });

            // Act
            logic.DownloadUrl("http://example.com");

            // Assert
            mockHttpClient.Verify(client => client.GetAsync(It.IsAny<string>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
