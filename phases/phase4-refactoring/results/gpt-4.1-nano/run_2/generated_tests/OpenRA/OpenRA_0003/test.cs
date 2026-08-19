using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;
using OpenRA.Game.Map;
using System.Collections.Immutable;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_Should_Handle_Successful_Response()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new ByteArrayContent(Encoding.UTF8.GetBytes("dummy map data"));
            responseContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "mapfile.map"
            };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Since the code creates HttpClient via HttpClientFactory.Create(), we need to replace it.
            // For this test, assume we can set a static property or method to override.
            // But since we can't modify the production code, this is a conceptual test.

            // Act
            var mapPreview = new MapPreview(/* initialize with necessary dependencies */);
            // Set the HttpClient to our mock client, if possible, or assume the code is refactored to allow injection.
            // For demonstration, suppose MapPreview has a constructor that accepts an HttpClient.
            // mapPreview.HttpClient = httpClient;

            // Call Install with a test URL
            await mapPreview.Install("http://testserver/maps/");

            // Assert
            // Verify that the response was processed, and the map data was handled.
            // For example, check that the status is set to Downloading or DownloadError accordingly.
            Assert.Equal(MapStatus.Downloading, mapPreview.Status);
        }
    }
}
