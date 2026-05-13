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
        // We need to test the RefreshServerList method and specifically the call to HttpClient.GetAsync.
        // Since HttpClient is created inside the method via HttpClientFactory.Create(), we cannot inject it directly.
        // We will mock HttpClientFactory.Create() using a partial class or by reflection if possible.
        // However, since the code is not designed for DI, we will test the method by overriding HttpClientFactory.Create via a helper.

        // To do this, we create a derived class that overrides HttpClientFactory.Create to return a mocked HttpClient.

        private class TestServerListLogic : ServerListLogic
        {
            private readonly HttpClient _httpClient;

            public TestServerListLogic(HttpClient httpClient)
                : base(new WidgetStub(), new ModDataStub(), gs => { })
            {
                _httpClient = httpClient;
            }

            // Override HttpClientFactory.Create to return our mocked HttpClient
            protected override HttpClient CreateHttpClient()
            {
                return _httpClient;
            }

            // Expose RefreshServerList for testing
            public new void RefreshServerList()
            {
                base.RefreshServerList();
            }
        }

        // Stub classes to satisfy constructor dependencies
        private class WidgetStub : OpenRA.Widgets.Widget
        {
            public override T Get<T>(string name) => default;
            public override T GetOrNull<T>(string name) => default;
        }

        private class ModDataStub : OpenRA.ModData
        {
            public override T GetOrCreate<T>() => (T)(object)new WebServicesStub();
        }

        private class WebServicesStub : OpenRA.Mods.Common.Widgets.Logic.WebServices
        {
            public override ModVersionStatus ModVersionStatus => ModVersionStatus.Latest;
        }

        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("yaml-content")
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

            // Wait a bit for the async Task.Run to execute
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
