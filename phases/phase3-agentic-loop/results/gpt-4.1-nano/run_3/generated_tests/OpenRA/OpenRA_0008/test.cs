using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common;

namespace OpenRA.Tests
{
    public class WebServicesTests
    {
        [Fact]
        public async Task CheckModVersion_CallsGetAsyncAndSetsStatus()
        {
            // Arrange
            var webServices = new WebServices();

            // Create a mock HttpMessageHandler
            var handlerMock = new Moq.Mock<HttpMessageHandler>(MockBehavior.Strict);

            // Setup the handler to respond with a specific content
            var responseContent = new StringContent("outdated");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            handlerMock
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(req => Task.FromResult(responseMessage));

            // Create HttpClient with the mocked handler
            var httpClient = new HttpClient(handlerMock.Object);

            // Since HttpClientFactory.Create() is static, we cannot replace it directly.
            // For testing, assume WebServices has a constructor that accepts an HttpClient.
            // Here, we will instantiate WebServices with a testable constructor.
            var testWebServices = new WebServicesWithHttpClient(httpClient);

            // Act
            testWebServices.CheckModVersion();

            // Wait for the async task to complete
            await Task.Delay(100); // small delay to allow task to run

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, testWebServices.ModVersionStatus);
        }
    }

    // Extending WebServices for testability
    public class WebServicesWithHttpClient : WebServices
    {
        private readonly HttpClient _httpClient;

        public WebServicesWithHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public new async void CheckModVersion()
        {
            await Task.Run(async () =>
            {
                var queryURL = new HttpQueryBuilder(VersionCheck)
                {
                    { "protocol", VersionCheckProtocol },
                    { "engine", Game.EngineVersion },
                    { "mod", Game.ModData.Manifest.Id },
                    { "version", Game.ModData.Manifest.Metadata.Version }
                }.ToString();

                try
                {
                    var httpResponseMessage = await _httpClient.GetAsync(queryURL);
                    var result = await httpResponseMessage.Content.ReadAsStringAsync();

                    var status = ModVersionStatus.Latest;
                    switch (result)
                    {
                        case "outdated": status = ModVersionStatus.Outdated; break;
                        case "unknown": status = ModVersionStatus.Unknown; break;
                        case "playtest": status = ModVersionStatus.PlaytestAvailable; break;
                    }

                    Game.RunAfterTick(() => ModVersionStatus = status);
                }
                catch { }
            });
        }
    }
}
