using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Server;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_CallsPostAsyncAndEnqueuesMessages()
        {
            // Arrange
            var mockHttpMessageHandler = new Moq.Protected.Mock<HttpMessageHandler>();
            var responseContent = "response text";

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPinger(factoryMock.Object);

            // Create a dummy server with minimal required properties
            var serverMock = new Mock<Server>();
            var modDataMock = new Mock<OpenRA.Server.WebServices>();
            modDataMock.Setup(m => m.ServerAdvertise).Returns("http://testserver");
            var modDataContainerMock = new Mock<OpenRA.Server.ModDataContainer>();
            modDataContainerMock.Setup(m => m.GetOrCreate<WebServices>()).Returns(modDataMock.Object);
            serverMock.Setup(s => s.ModData).Returns(modDataContainerMock.Object);
            serverMock.Setup(s => s.Settings).Returns(new ServerSettings { AdvertiseOnline = true, AdvertiseOnLocalNetwork = false });
            serverMock.Setup(s => s.IsMultiplayer).Returns(true);

            // Act
            pinger.UpdateMasterServer(serverMock.Object, "testPostData");

            // Wait for the async task to complete
            await Task.Delay(100);

            // Assert
            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString() == "http://testserver"),
                ItExpr.IsAny<System.Threading.CancellationToken>());

            // Check that the message was enqueued
            Assert.Contains(MasterServerPinger.Connected, pinger.masterServerMessages);
        }
    }
}
