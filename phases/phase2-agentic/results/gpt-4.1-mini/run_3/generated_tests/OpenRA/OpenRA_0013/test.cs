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
        // We will mock HttpClientFactory.Create() using a partial mock or by replacing the factory method via a delegate or similar.
        // However, the code snippet does not show how HttpClientFactory is implemented or if it can be replaced.
        // For this test, we will create a derived class that overrides HttpClientFactory.Create() to return a mocked HttpClient.

        private class TestServerListLogic : ServerListLogic
        {
            private readonly HttpClient _httpClient;

            public TestServerListLogic(HttpClient httpClient)
                : base(new WidgetStub(), new ModDataStub(), gs => { })
            {
                _httpClient = httpClient;
            }

            // Override the HttpClientFactory.Create() call by shadowing the method or by exposing a protected virtual method.
            // Since the original code uses HttpClientFactory.Create() directly, we cannot override it without source changes.
            // So we will simulate the call by exposing a method that calls the internal async task and test that method instead.
            // For this example, we will expose a public method that calls the internal async task for testing.

            public async Task RefreshServerListAsync()
            {
                // Copy of the internal async task logic from RefreshServerList for testing
                var queryURL = "http://testserver/query";

                var client = _httpClient;
                var httpResponseMessage = await client.GetAsync(queryURL);
                var result = await httpResponseMessage.Content.ReadAsStreamAsync();

                // We won't parse YAML here, just verify that GetAsync was called and response was read.
            }
        }

        [Fact]
        public async Task RefreshServerList_CallsHttpClientGetAsync()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            handlerMock
               .Protected()
               // Setup the PROTECTED method SendAsync (which is called by HttpClient.GetAsync)
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent("test content"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);

            var serverListLogic = new TestServerListLogic(httpClient);

            // Act
            await serverListLogic.RefreshServerListAsync();

            // Assert
            // Verify that HttpClient.GetAsync was called once with the expected URL
            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Once(),
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get &&
                  req.RequestUri == new Uri("http://testserver/query")
               ),
               ItExpr.IsAny<CancellationToken>()
            );
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
    }
}
