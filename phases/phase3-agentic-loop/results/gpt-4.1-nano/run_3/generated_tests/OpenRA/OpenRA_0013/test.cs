using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Widgets.Logic;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        private class DummyWidget : Widget
        {
            private readonly Dictionary<string, Widget> children = new Dictionary<string, Widget>();
            public override T Get<T>(string name) => children.TryGetValue(name, out var widget) ? (T)(object)widget : default;
            public override T GetOrNull<T>(string name) => Get<T>(name);
            public void AddChild(string name, Widget widget) => children[name] = widget;
        }

        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var responseContent = new StringContent("dummy");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(f => f.Create())
                .Returns(httpClient);

            var widget = new DummyWidget();
            var modData = new Mock<ModData>().Object;
            var onJoin = new Action<GameServer>(_ => { });

            var serverListLogic = new ServerListLogic(widget, modData, onJoin)
            {
                // Inject the factory
                HttpClientFactory = mockHttpClientFactory.Object
            };

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.AtLeastOnce);
        }
    }
}
