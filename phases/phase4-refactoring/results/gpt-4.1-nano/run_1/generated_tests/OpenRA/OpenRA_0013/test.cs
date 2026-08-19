using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OpenRA.Mods.Common.Widgets.Logic;

namespace OpenRA.Tests
{
    public class ServerListLogicTests
    {
        // Interface to abstract HttpClient for easier mocking
        public interface IHttpClientWrapper
        {
            Task<HttpResponseMessage> GetAsync(string url);
        }

        // Subclass to inject mock dependencies
        public class ServerListLogicTestable : ServerListLogic
        {
            public static Func<IHttpClientWrapper> HttpClientFactory { get; set; } = () => throw new NotImplementedException();

            public ServerListLogicTestable(Widget widget, ModData modData, Action<GameServer> onJoin)
                : base(widget, modData, onJoin)
            {
            }

            protected override IHttpClientWrapper CreateHttpClient()
            {
                return HttpClientFactory();
            }
        }

        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var mockHttpClient = new Mock<IHttpClientWrapper>();
            var mockResponse = new HttpResponseMessage
            {
                Content = new StringContent("dummy")
            };
            bool getAsyncCalled = false;
            mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(mockResponse)
                .Callback(() => getAsyncCalled = true);

            ServerListLogicTestable.HttpClientFactory = () => mockHttpClient.Object;

            var mockWidget = new Mock<Widget>();
            var mockModData = new Mock<ModData>();
            var logic = new ServerListLogicTestable(mockWidget.Object, mockModData.Object, s => { });

            // Act
            await logic.RefreshServerList();

            // Assert
            Assert.True(getAsyncCalled, "HttpClient.GetAsync was not called");
        }
    }
}
