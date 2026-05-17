using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Mods.Common.Tests.ServerTraits
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_PostAsync_Success()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[0]Success")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockServer = new Mock<S>();
            var mockModData = new Mock<ModData>();
            var mockWebServices = new Mock<WebServices>();
            mockWebServices.Setup(ws => ws.ServerAdvertise).Returns("https://example.com/ping");
            mockModData.Setup(md => md.GetOrCreate<WebServices>()).Returns(mockWebServices.Object);
            mockServer.Setup(s => s.ModData).Returns(mockModData.Object);

            var pinger = new MasterServerPinger();

            // Act
            await pinger.UpdateMasterServer(mockServer.Object, "postData");

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://example.com/ping")
                ),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            );
        }

        [Fact]
        public async Task UpdateMasterServer_PostAsync_Failure()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockServer = new Mock<S>();
            var mockModData = new Mock<ModData>();
            var mockWebServices = new Mock<WebServices>();
            mockWebServices.Setup(ws => ws.ServerAdvertise).Returns("https://example.com/ping");
            mockModData.Setup(md => md.GetOrCreate<WebServices>()).Returns(mockWebServices.Object);
            mockServer.Setup(s => s.ModData).Returns(mockModData.Object);

            var pinger = new MasterServerPinger();

            // Act
            await pinger.UpdateMasterServer(mockServer.Object, "postData");

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri == new Uri("https://example.com/ping")
                ),
                ItExpr.IsAny<System.Threading.CancellationToken>()
            );
        }
    }
}
