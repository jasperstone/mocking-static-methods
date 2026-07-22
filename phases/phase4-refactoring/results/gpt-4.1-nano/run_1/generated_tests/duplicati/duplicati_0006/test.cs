using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Duplicati.Library;

namespace Duplicati.Tests
{
    public class OAuthHttpClientTests
    {
        private class DummyAuthenticator : OAuthHttpMessageHandler
        {
            public override HttpRequestMessage PreventAuthentication(HttpRequestMessage request)
            {
                // Just return the request for testing
                return request;
            }
        }

        [Fact]
        public async Task GetAsync_Should_Throw_TimeoutException_On_OperationCanceledException()
        {
            // Arrange
            var handler = new DummyAuthenticator();
            var client = new OAuthHttpClient(handler);
            // Set a small timeout for testing
            client.Timeout = TimeSpan.FromMilliseconds(1);
            var requestUri = "http://test";

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.GetAsync(requestUri);
            });
        }

        [Fact]
        public async Task SendAsync_Should_Call_SendAsync_And_Handle_Timeout()
        {
            // Arrange
            var mockAuthenticator = new Mock<OAuthHttpMessageHandler>("authid", "protocol", "url");
            var handler = mockAuthenticator.Object;
            var client = new OAuthHttpClient(handler);
            client.Timeout = TimeSpan.FromMilliseconds(1);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Setup the mock to throw OperationCanceledException when called
            var mockClient = new Mock<OAuthHttpClient>(handler);
            mockClient.CallBase = true;
            mockClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>(), true, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await mockClient.Object.SendAsync(request, true, cts.Token);
            });
        }
    }
}
