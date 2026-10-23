using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Threading;
using OpenRA.Mods.Common.Server;
using OpenRA.Network;
using OpenRA.Server;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        class TestMasterServerPinger : MasterServerPinger
        {
            private readonly HttpClient _httpClient;

            public TestMasterServerPinger(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            protected override HttpClient CreateHttpClient()
            {
                return _httpClient;
            }
        }

        [Fact]
        public async Task UpdateMasterServer_CallsPostAsync_WithExpectedParameters()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            var pinger = new TestMasterServerPinger(mockHttpClient);

            // Setup the mock to respond with a dummy response
            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("OK")
                    };
                    return await Task.FromResult(response);
                });

            // Create a dummy server object with minimal properties
            var server = new Mock<S>();
            var modData = new Mock<OpenRA.ModData>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns("http://testserver");
            modData.Setup(md => md.GetOrCreate<WebServices>()).Returns(webServices.Object);
            server.Setup(s => s.ModData).Returns(modData.Object);
            server.Setup(s => s.Settings).Returns(new ServerSettings
            {
                AdvertiseOnline = true,
                AdvertiseOnLocalNetwork = false
            });
            server.Setup(s => s.IsMultiplayer).Returns(true);

            string postData = "testdata";

            // Act
            await pinger.UpdateMasterServerAsync(server.Object, postData);

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://testserver" &&
                req.Content.ReadAsStringAsync().Result == postData
            )), Times.Once);
        }
    }
}
