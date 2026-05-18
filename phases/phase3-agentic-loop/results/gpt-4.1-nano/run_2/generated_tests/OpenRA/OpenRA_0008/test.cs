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

            // Mock HttpMessageHandler to intercept SendAsync
            var mockHandler = new Moq.Mock<HttpMessageHandler>();
            mockHandler
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("outdated")
                });

            var client = new HttpClient(mockHandler.Object);

            // Replace the factory method to return our mock client
            HttpClientFactory.SetFactory(() => client);

            // Act
            webServices.CheckModVersion();

            // Wait for the async task to complete
            await Task.Delay(100);

            // Assert
            Assert.Equal(ModVersionStatus.Outdated, webServices.ModVersionStatus);
        }
    }

    // Helper to override HttpClientFactory for testing
    public static class HttpClientFactory
    {
        private static Func<HttpClient> _factory;

        public static void SetFactory(Func<HttpClient> factory)
        {
            _factory = factory;
        }

        public static HttpClient Create()
        {
            if (_factory != null)
                return _factory();
            return new HttpClient();
        }
    }
}
