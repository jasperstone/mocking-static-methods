using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using OpenRA.Mods.Common.Widgets.Logic;
using Xunit;

namespace OpenRA.Mods.Common.Tests.Widgets.Logic
{
    public class ServerListLogicTests
    {
        // We need to test the RefreshServerList method, especially the call to HttpClient.GetAsync.
        // Since HttpClient is created inside the method via HttpClientFactory.Create(),
        // we will mock HttpClientFactory.Create() to return a HttpClient with a mocked HttpMessageHandler.

        // To do this, we will create a derived class of ServerListLogic that overrides HttpClientFactory.Create()
        // to return our mocked HttpClient.

        private class TestServerListLogic : ServerListLogic
        {
            private readonly HttpClient _httpClient;

            public TestServerListLogic(HttpClient httpClient)
                : base(new DummyWidget(), new DummyModData(), _ => { })
            {
                _httpClient = httpClient;
            }

            // Override the HttpClientFactory.Create method to return our mocked HttpClient
            protected override HttpClient CreateHttpClient()
            {
                return _httpClient;
            }
        }

        // Dummy classes to satisfy constructor dependencies
        private class DummyWidget : OpenRA.Widgets.Widget
        {
            public override T Get<T>(string name) => default;
            public override OpenRA.Widgets.Widget GetOrNull(string name) => null;
        }

        private class DummyModData : OpenRA.ModData
        {
            public override T GetOrCreate<T>() => default;
        }

        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("dummy yaml content")
            };

            handlerMock
               .Protected()
               // Setup the PROTECTED method SendAsync (which is called by HttpClient.GetAsync)
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(response)
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var logic = new TestServerListLogic(httpClient);

            // Act
            logic.RefreshServerList();

            // Wait a bit for the async Task.Run to complete
            await Task.Delay(100);

            // Assert
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.AtLeastOnce(),
               ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
