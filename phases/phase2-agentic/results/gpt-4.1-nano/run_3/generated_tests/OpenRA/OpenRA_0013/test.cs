using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        private class DummyWidget : Widget
        {
            private readonly Dictionary<string, Widget> children = new();
            public override T Get<T>(string name) => children.TryGetValue(name, out var widget) ? (T)(object)widget : default;
            public override T GetOrNull<T>(string name) => Get<T>(name);
            public void AddChild(string name, Widget widget) => children[name] = widget;
        }

        private class DummyContent : Widget
        {
            public Stream ContentStream { get; set; }
        }

        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var dummyStream = new MemoryStream();
            var dummyContent = new DummyContent { ContentStream = dummyStream };
            var responseMessage = new HttpResponseMessage
            {
                Content = new StreamContent(dummyStream)
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var widget = new DummyWidget();
            var modDataMock = new Mock<ModData>();
            var onJoin = new Action<GameServer>(_ => { });

            var serverListLogic = new ServerListLogic(widget, modDataMock.Object, onJoin)
            {
                // Inject the factory
                HttpClientFactory = factoryMock.Object
            };

            // Act
            await serverListLogic.RefreshServerList();

            // Assert
            mockHttpMessageHandler.Verify(m => m.Send(It.IsAny<HttpRequestMessage>()), Times.Once);
        }
    }

    // Extension to inject custom HttpClientFactory for testing
    public static class ServerListLogicExtensions
    {
        public static HttpClientFactory HttpClientFactory { get; set; }
    }
}
