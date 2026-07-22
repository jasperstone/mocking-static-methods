using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Net;
using OpenRA.Mods.Common.Server;
using OpenRA.Network;
using OpenRA.Server;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        class DummyHttpResponseMessage : HttpResponseMessage
        {
            public DummyHttpResponseMessage(string content)
            {
                Content = new StringContent(content);
            }
        }

        [Fact]
        public async Task UpdateMasterServer_PostAsync_Called_With_Correct_Parameters()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = "OK";
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var response = new DummyHttpResponseMessage(responseContent);
                    return await Task.FromResult(response);
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPinger(mockFactory.Object);

            // Setup a dummy server with necessary properties
            var mockServer = new Mock<S>();
            var mockModData = new Mock<OpenRA.Mods.Common.WebServices>();
            mockModData.Setup(m => m.ServerAdvertise).Returns("http://testserver/advertise");
            var mockModDataContainer = new Mock<OpenRA.Mods.Common.WebServices>();
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            mockServer.Setup(s => s.ModData.GetOrCreate<OpenRA.Mods.Common.WebServices>()).Returns(mockModData.Object);
            // Call the method under test
            await pinger.UpdateMasterServer(mockServer.Object, "testPostData");
            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://testserver/advertise"
            )), Times.Once);
        }
    }
}
