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
        public async Task UpdateMasterServer_PostAsync_Called_WithCorrectParameters()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new StringContent("response");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPinger();

            // Inject the mocked factory into the static property (assuming it's a static property or field)
            // Since the original code does not show how HttpClientFactory is injected, 
            // we need to assume or modify the class to allow injection for testing.
            // For this example, let's assume there's a static property or method to set the factory.
            // If not, we might need to refactor the class to allow dependency injection.

            // For demonstration, suppose we add a static property for testing:
            // MasterServerPinger.HttpClientFactory = factoryMock.Object;

            // Mock server setup
            var serverMock = new Mock<OpenRA.Server.Server>();
            var modDataMock = new Mock<OpenRA.Server.ModData>();
            var webServicesMock = new Mock<OpenRA.Server.WebServices>();
            webServicesMock.Setup(ws => ws.ServerAdvertise).Returns("http://testendpoint");
            modDataMock.Setup(md => md.GetOrCreate<OpenRA.Server.WebServices>()).Returns(webServicesMock.Object);
            serverMock.Setup(s => s.ModData).Returns(modDataMock.Object);

            string postData = "testData";

            // Act
            await pinger.UpdateMasterServer(serverMock.Object, postData);

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://testendpoint"
            )), Times.Once);
        }
    }
}
