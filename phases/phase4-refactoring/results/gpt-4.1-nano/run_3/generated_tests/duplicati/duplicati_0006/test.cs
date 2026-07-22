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
        private class MockOAuthHttpMessageHandler : OAuthHttpMessageHandler
        {
            public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Simulate a successful response
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetAsync_Should_Throw_TimeoutException_On_OperationCanceled()
        {
            // Arrange
            var handler = new Mock<OAuthHttpMessageHandler>();
            handler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var client = new OAuthHttpClient(handler.Object);
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
            var authenticator = new MockOAuthHttpMessageHandler();
            var client = new OAuthHttpClient(authenticator);
            client.Timeout = TimeSpan.FromMilliseconds(10);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var cts = new CancellationTokenSource();

            // Setup the inner SendAsync to throw OperationCanceledException
            var mockHandler = new Mock<OAuthHttpMessageHandler>();
            mockHandler.Setup(h => h.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var testClient = new OAuthHttpClient(mockHandler.Object);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                await testClient.SendAsync(request, true, cts.Token);
            });
        }

        [Fact]
        public void PreventAuthentication_Should_Call_PreventAuthentication()
        {
            // Arrange
            var mockAuthenticator = new Mock<OAuthHttpMessageHandler>();
            var client = new OAuthHttpClient(mockAuthenticator.Object);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            client.PreventAuthentication(request);

            // Assert
            mockAuthenticator.Verify(h => h.PreventAuthentication(It.Is<HttpRequestMessage>(r => r == request)), Times.Once);
        }
    }
}
