using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_PostAsyncCalled()
        {
            // Arrange
            var server = new Mock<S>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns("https://example.com");
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices.Object);
            var postData = "test data";
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(hcf => hcf.Create()).Returns(httpClient);

            // Act
            var masterServerPinger = new MasterServerPinger();
            await masterServerPinger.UpdateMasterServer(server.Object, postData);

            // Assert
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }
    }
}
