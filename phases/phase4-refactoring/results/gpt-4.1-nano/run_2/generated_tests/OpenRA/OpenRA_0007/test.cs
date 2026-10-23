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
        [Fact]
        public async Task UpdateMasterServer_CallsPostAsync_WithExpectedParameters()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("response")
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(mockResponse));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Create a dummy server object with necessary properties
            var server = new Mock<S>();
            var modData = new Mock<OpenRA.Mods.Common.Server.WebServices>();
            modData.Setup(m => m.ServerAdvertise).Returns("http://testserver/advertise");
            var modDataContainer = new Mock<OpenRA.Mods.Common.Server.WebServicesContainer>();
            modDataContainer.Setup(m => m.GetOrCreate<WebServices>()).Returns(modData.Object);
            server.Setup(s => s.ModData).Returns(modDataContainer.Object);
            server.Setup(s => s.IsMultiplayer).Returns(true);
            server.Setup(s => s.Settings).Returns(new ServerSettings
            {
                AdvertiseOnline = true,
                AdvertiseOnLocalNetwork = false
            });
            server.Setup(s => s.SendFluentMessage(It.IsAny<string>()));

            var pinger = new MasterServerPinger();

            // Act
            await pinger.UpdateMasterServerAsync(server.Object, "testPostData", httpClient);

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://testserver/advertise"
            )), Times.Once);
        }
    }
}
