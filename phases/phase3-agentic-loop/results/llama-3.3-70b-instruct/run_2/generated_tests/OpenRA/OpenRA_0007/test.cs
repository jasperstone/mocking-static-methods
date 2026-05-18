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
            var serverMock = new Mock<OpenRA.Server.Server>();
            var webServicesMock = new Mock<OpenRA.Mods.Common.WebServices>();
            webServicesMock.Setup(ws => ws.ServerAdvertise).Returns("https://example.com");
            serverMock.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(webServicesMock.Object);

            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(httpClientHandlerMock.Object.SendAsync);

            var masterServerPinger = new MasterServerPinger();

            // Act
            await masterServerPinger.UpdateMasterServer(serverMock.Object, "postData");

            // Assert
            httpClientMock.Verify(h => h.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMasterServer_PostAsyncFailed()
        {
            // Arrange
            var serverMock = new Mock<OpenRA.Server.Server>();
            var webServicesMock = new Mock<OpenRA.Mods.Common.WebServices>();
            webServicesMock.Setup(ws => ws.ServerAdvertise).Returns("https://example.com");
            serverMock.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(webServicesMock.Object);

            var httpClientMock = new Mock<HttpClient>();
            var httpClientHandlerMock = new Mock<HttpMessageHandler>();
            httpClientHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws(new HttpRequestException());
            httpClientMock
                .Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(httpClientHandlerMock.Object.SendAsync);

            var masterServerPinger = new MasterServerPinger();

            // Act and Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => masterServerPinger.UpdateMasterServer(serverMock.Object, "postData"));
        }
    }
}
