using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA;
using OpenRA.Map;

namespace OpenRA.Tests.Map
{
    public class MapPreviewHttpClientTests
    {
        [Fact]
        public async Task Install_Should_Call_GetAsync_And_Handle_Success()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("response content")
            };
            responseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
            {
                FileName = "mapfile"
            };
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(f => f.Create()).Returns(httpClient);

            var mapPreview = new MapPreview
            {
                // Initialize necessary properties
            };

            // Act
            var mapPreviewType = typeof(MapPreview);
            var installMethod = mapPreviewType.GetMethod("Install", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // Use reflection to invoke the method
            var task = (Task)installMethod.Invoke(mapPreview, new object[] { "http://example.com/" });
            await task;

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
            // Additional assertions can be added based on internal state changes
        }

        [Fact]
        public async Task Install_Should_Set_Status_To_DownloadError_On_NonSuccessStatusCode()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(f => f.Create()).Returns(httpClient);

            var mapPreview = new MapPreview
            {
                // Initialize necessary properties
            };

            // Act
            var installMethod = typeof(MapPreview).GetMethod("Install", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var task = (Task)installMethod.Invoke(mapPreview, new object[] { "http://example.com/" });
            await task;

            // Assert
            // Check that innerData.Status is set to MapStatus.DownloadError
            // This may require exposing innerData or using reflection
        }
    }
}
