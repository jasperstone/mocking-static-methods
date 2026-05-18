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
        public async Task UpdateMasterServer_PostAsync_Called_WithExpectedParameters()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var responseContent = new StringContent("response");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            // Setup protected method SendAsync via reflection
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPinger
            {
                // Inject the mocked HttpClientFactory
                HttpClientFactory = factoryMock.Object,
                // Set up other necessary properties
                masterServerMessages = new System.Collections.Generic.Queue<string>(),
                isInitialPing = true,
            };

            var serverMock = new Mock<OpenRA.Server.Server>();
            var modDataMock = new Mock<OpenRA.Server.ModData>();
            var webServicesMock = new Mock<OpenRA.Server.WebServices>();
            var serverSettingsMock = new Mock<OpenRA.Server.Settings>();
            var serverModData = new OpenRA.Server.ModData();
            var serverWebServices = new OpenRA.Server.WebServices
            {
                ServerAdvertise = "http://testendpoint"
            };

            // Setup the chain of calls
            serverMock.Setup(s => s.ModData).Returns(serverModData);
            serverModData.Setup(m => m.GetOrCreate<OpenRA.Server.WebServices>()).Returns(webServicesMock.Object);
            webServicesMock.Setup(w => w.ServerAdvertise).Returns("http://testendpoint");
            serverMock.Setup(s => s.Settings).Returns(new OpenRA.Server.Settings());

            // Act
            await pinger.UpdateMasterServer(serverMock.Object, "testPostData");

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://testendpoint"
            )), Times.Once);
        }
    }
}
