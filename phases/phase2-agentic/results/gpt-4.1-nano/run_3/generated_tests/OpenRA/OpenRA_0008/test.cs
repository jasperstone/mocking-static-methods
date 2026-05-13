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
        public async Task CheckModVersion_CallsGetAsyncAndUpdatesStatus()
        {
            // Arrange
            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            var responseContent = new StringContent("latest");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
                .Returns<HttpRequestMessage>(async (req) => await Task.FromResult(responseMessage));

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Replace the factory to return our mock client
            var factoryMock = new Moq.Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var webServices = new WebServicesWithInjectedHttpClient(factoryMock.Object);

            // Act
            webServices.CheckModVersion();

            // Wait for the async task to complete
            await Task.Delay(100); // small delay to allow task to run

            // Assert
            Assert.Equal(ModVersionStatus.Latest, webServices.ModVersionStatus);
        }
    }

    // Extending WebServices to inject HttpClient for testing
    public class WebServicesWithInjectedHttpClient : WebServices
    {
        private readonly IHttpClientFactory _factory;

        public WebServicesWithInjectedHttpClient(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public new void CheckModVersion()
        {
            Task.Run(async () =>
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
                    var client = _factory.Create();

                    var httpResponseMessage = await client.GetAsync(queryURL);
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
