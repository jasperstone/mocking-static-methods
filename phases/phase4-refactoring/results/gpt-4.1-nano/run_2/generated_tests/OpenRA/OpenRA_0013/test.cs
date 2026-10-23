using Xunit;
using Moq;
using System.Net.Http;
using System.Threading.Tasks;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Network;
using OpenRA.Support;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Setup the mock response
            var responseContent = new MemoryStream(Encoding.UTF8.GetBytes("yaml content"));
            var responseMessage = new HttpResponseMessage
            {
                Content = new StreamContent(responseContent)
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) =>
                {
                    return responseMessage;
                });

            // Replace the factory method to return our mock client
            // Assuming we can set a static delegate for test purposes
            HttpClientFactory.Create = () => mockHttpClient;

            var widget = new Mock<Widget>();
            var modData = new Mock<ModData>();
            var onJoin = new Action<GameServer>(_ => { });

            var serverListLogic = new ServerListLogic(widget.Object, modData.Object, onJoin);

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.AtLeastOnce);
        }
    }
}
