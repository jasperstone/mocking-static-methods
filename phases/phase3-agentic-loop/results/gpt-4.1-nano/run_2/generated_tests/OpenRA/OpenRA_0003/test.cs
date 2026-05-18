using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using OpenRA.Game.Map;

namespace OpenRA.Tests
{
    public class MapPreviewTests
    {
        [Fact]
        public async Task Install_Should_Call_HttpClient_GetAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("response content")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(_ => _.Create()).Returns(httpClient);

            // Instantiate MapPreview with dependencies
            var mapPreview = new MapPreview(/* constructor parameters as needed */);
            // Note: You need to set the HttpClientFactory or equivalent in MapPreview
            // depending on its implementation. For this example, assume you can inject it.

            // Act
            await mapPreview.Install("http://example.com/maps/");

            // Assert
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == HttpMethod.Get
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
