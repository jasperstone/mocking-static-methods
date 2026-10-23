using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA;
using OpenRA.Map;
using System.Collections.Immutable;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_Should_Call_GetAsync_And_Handle_Success_Response()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
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
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    return await Task.FromResult(responseMessage);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Create a MapPreview instance with dependencies injected
            var mapPreview = new MapPreview(/* dependencies */);
            // For this example, assume we can set the HttpClient or factory
            // Since the original code creates HttpClient via factory, we need to simulate that
            // For simplicity, assume MapPreview has a constructor accepting HttpClient (not in original code)
            // or that we can set a property to override the factory behavior.

            // For demonstration, suppose we can set a static factory method or property
            // (In real code, refactoring would be needed to make this testable)

            // Act
            await mapPreview.Install("http://example.com/maps/", /* other params as needed */);

            // Assert
            // Verify that GetAsync was called with the correct URL
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get && req.RequestUri.ToString() == "http://example.com/maps/")), Times.Once);
        }
    }
}
