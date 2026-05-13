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
        public async Task UpdateMasterServer_PostAsync_CallsHttpClientPostAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("response")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockFactory = new Moq.Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPinger();
            // Inject the mocked HttpClientFactory
            typeof(MasterServerPinger).GetField("HttpClientFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .SetValue(null, mockFactory.Object);

            var serverMock = new Moq.Mock<Server>();
            var modDataMock = new Moq.Mock<OpenRA.Server.ModData>();
            var webServicesMock = new Moq.Mock<WebServices>();
            var serverInstance = serverMock.Object;

            // Setup server.ModData.GetOrCreate<WebServices>() to return webServicesMock.Object
            var modData = new Moq.Mock<OpenRA.Server.ModData>();
            var webServices = new WebServices { ServerAdvertise = "http://testendpoint" };
            modData.Setup(m => m.GetOrCreate<WebServices>()).Returns(webServices);
            serverMock.Setup(s => s.ModData).Returns(modData.Object);

            string postData = "test data";

            // Act
            await pinger.UpdateMasterServer(serverInstance, postData);

            // Assert
            mockHttpMessageHandler.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post && req.RequestUri.ToString() == webServices.ServerAdvertise
            )), Times.Once);
        }
    }
}
