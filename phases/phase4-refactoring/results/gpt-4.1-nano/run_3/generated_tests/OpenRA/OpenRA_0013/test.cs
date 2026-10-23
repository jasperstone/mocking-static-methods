using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;

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

            var mockFactory = new Mock<IHttpClientFactory>();
            mockFactory.Setup(f => f.Create()).Returns(mockHttpClient);

            // Create a dummy widget and modData
            var widget = new DummyWidget();
            var modData = new DummyModData();

            // Create the ServerListLogic instance
            var logic = new ServerListLogic(widget, modData, server => { /* no-op */ });

            // Setup the HttpResponseMessage to return a stream
            var responseContent = new StringContent("dummy");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (request) =>
                {
                    return responseMessage;
                });

            // Act
            await logic.RefreshServerList();

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get
            )), Times.Once);
        }
    }

    // Dummy implementations for Widget and ModData to instantiate ServerListLogic
    public class DummyWidget : Widget
    {
        public override T Get<T>(string name) => default;
        public override T GetOrNull<T>(string name) => default;
        public override string Name => "dummy";
        public override T Get<T>(string name, T defaultValue) => default;
        public override T GetOrNull<T>(string name, T defaultValue) => default;
        public override Bounds Bounds { get; set; }
    }

    public class DummyModData : ModData
    {
        public override T GetOrCreate<T>() => default;
    }
}
