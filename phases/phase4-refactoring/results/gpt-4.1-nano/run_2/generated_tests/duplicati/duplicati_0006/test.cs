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
        public async Task GetAsync_Should_Throw_TimeoutException_On_OperationCanceled()
        {
            // Arrange
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(mockHandler.Object);
            client.Timeout = TimeSpan.FromMilliseconds(10);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await client.GetAsync("http://test");
            });
        }

        [Fact]
        public async Task SendAsync_Should_Throw_TimeoutException_On_OperationCanceled()
        {
            // Arrange
            var authenticator = new DummyAuthenticator();
            var client = new OAuthHttpClient(authenticator);
            client.Timeout = TimeSpan.FromMilliseconds(10);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Setup the inner SendAsync to throw OperationCanceledException
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpCompletionOption>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var testClient = new OAuthHttpClient(mockHandler.Object);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await testClient.SendAsync(request, true, cts.Token);
            });
        }
    }
}
