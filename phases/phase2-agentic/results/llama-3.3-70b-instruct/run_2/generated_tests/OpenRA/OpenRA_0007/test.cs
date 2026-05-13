using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using Xunit;

namespace OpenRA.Mods.Common.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_PostAsyncCalled()
        {
            // Arrange
            var server = new Mock<S>();
            var webServices = new Mock<WebServices>();
            webServices.Setup(ws => ws.ServerAdvertise).Returns("https://example.com");
            server.Setup(s => s.ModData.GetOrCreate<WebServices>()).Returns(webServices.Object);
            var masterServerPinger = new MasterServerPinger();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var httpClient = new Mock<HttpClient>();
            var httpResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            httpClient.Setup(hc => hc.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(httpResponseMessage);
            httpClientFactory.Setup(hcf => hcf.CreateClient()).Returns(httpClient.Object);
            HttpClientFactory.SetInstance(httpClientFactory.Object);

            // Act
            await masterServerPinger.UpdateMasterServer(server.Object, "postData");

            // Assert
            httpClient.Verify(hc => hc.PostAsync("https://example.com", It.IsAny<StringContent>()), Times.Once);
        }
    }
}
