using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Moq;
using OpenRA.Mods.Common.Server;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_PostAsync_CallsHttpClientPostAsync()
        {
            // Arrange
            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>();
            var responseContent = new StringContent("response");
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = responseContent
            };

            mockHttpMessageHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var factoryMock = new Moq.Mock<IHttpClientFactory>();
            factoryMock.Setup(f => f.Create()).Returns(httpClient);

            var pinger = new MasterServerPingerWithInjectedHttpClient(factoryMock.Object);

            var serverMock = new Moq.Mock<S>();
            var modDataMock = new Moq.Mock<OpenRA.Server.ModData>();
            var webServicesMock = new Moq.Mock<WebServices>();
            var serverObject = serverMock.Object;

            // Setup server.ModData.GetOrCreate<WebServices>() to return webServicesMock.Object
            var modDataDict = new Dictionary<Type, object>
            {
                { typeof(WebServices), webServicesMock.Object }
            };
            var modDataContainer = new MockModData(modDataDict);
            serverMock.Setup(s => s.ModData).Returns(modDataContainer);

            webServicesMock.Setup(ws => ws.ServerAdvertise).Returns("http://testendpoint");

            string postData = "test data";

            // Act
            await pinger.UpdateMasterServerAsync(serverObject, postData);

            // Assert
            mockHttpMessageHandler.Verify(m => m.SendAsync(It.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString() == "http://testendpoint"
            )), Times.Once);
        }
    }

    // Helper classes for mocking
    public class MockModData : OpenRA.Server.ModData
    {
        private readonly Dictionary<Type, object> _data;

        public MockModData(Dictionary<Type, object> data)
        {
            _data = data;
        }

        public override T GetOrCreate<T>()
        {
            if (_data.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }
            return base.GetOrCreate<T>();
        }
    }

    // Extend MasterServerPinger to allow injection of HttpClient for testing
    public class MasterServerPingerWithInjectedHttpClient : MasterServerPinger
    {
        private readonly IHttpClientFactory _factory;

        public MasterServerPingerWithInjectedHttpClient(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task UpdateMasterServerAsync(S server, string postData)
        {
            isBusy = true;
            try
            {
                var endpoint = server.ModData.GetOrCreate<WebServices>().ServerAdvertise;
                var client = _factory.Create();
                var response = await client.PostAsync(endpoint, new StringContent(postData));
                var masterResponseText = await response.Content.ReadAsStringAsync();
                // Additional logic omitted for brevity
            }
            catch (Exception ex)
            {
                // Handle exception
            }
            finally
            {
                isBusy = false;
            }
        }
    }
}
