using Moq;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq.Contrib.HttpClient;
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
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Setup<Task<HttpResponseMessage>>(
                    h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(new HttpResponseMessage());

            var httpClient = new HttpClient(handlerMock.Object);

            var masterServerPinger = new MasterServerPinger();

            // Act
            await masterServerPinger.UpdateMasterServer(serverMock.Object, "postData");

            // Assert
            handlerMock.Verify(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
