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
        public async Task UpdateMasterServer_CallsPostAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(handlerMock.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPinger();

            // Here, assuming we can set the factory or inject the HttpClient
            // Since the class does not support this, this is a conceptual test

            var serverMock = new Moq.Mock<OpenRA.Server.Server>();
            var modDataMock = new Moq.Mock<OpenRA.Server.ModData>();
            var webServicesMock = new Moq.Mock<OpenRA.Server.WebServices>();
            var responseContentMock = new Moq.Mock<HttpContent>();
            var responseMock = new Moq.Mock<HttpResponseMessage>();
            var responseString = "test response";

            // Setup server.ModData.GetOrCreate<WebServices>() to return webServicesMock.Object
            var mockModData = new Moq.Mock<OpenRA.Server.ModData>();
            mockModData.Setup(m => m.GetOrCreate<WebServices>()).Returns(webServicesMock.Object);
            serverMock.Setup(s => s.ModData).Returns(mockModData.Object);
            serverMock.Setup(s => s.Settings).Returns(new OpenRA.Server.ServerSettings { AdvertiseOnline = true, AdvertiseOnLocalNetwork = false });
            serverMock.Setup(s => s.IsMultiplayer).Returns(true);
            serverMock.Setup(s => s.SendFluentMessage(It.IsAny<string>()));

            var endpointUri = new Uri("http://testendpoint");
            webServicesMock.Setup(ws => ws.ServerAdvertise).Returns(endpointUri);

            var responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(responseString)
            };

            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req => responseMessage);

            // Act
            await pinger.UpdateMasterServer(serverMock.Object, "postData");

            // Assert
            handlerMock.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri == endpointUri
            )), Times.Once);
        }
    }
}
