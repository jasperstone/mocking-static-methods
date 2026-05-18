using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

namespace Duplicati.Library.Backend.OAuthHelper.Tests
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_AuthenticateTrue_SuccessfulResponse()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(new HttpResponseMessage(HttpStatusCode.OK));
            var client = new TestOAuthHttpClient(mockHandler);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();

            // Act
            var response = await client.SendAsync(request, authenticate: true, cts.Token);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_AuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupResponse(new HttpResponseMessage(HttpStatusCode.OK));
            var client = new TestOAuthHttpClient(mockHandler);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var cts = new CancellationTokenSource();

            // Act
            var response = await client.SendAsync(request, authenticate: false, cts.Token);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(mockHandler.PreventAuthenticationCalled);
        }

        [Fact]
        public async Task SendAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupDelayedResponse(TimeSpan.FromMilliseconds(100));
            var client = new TestOAuthHttpClient(mockHandler);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => client.SendAsync(request, true, cts.Token));
        }

        [Fact]
        public async Task SendAsync_TimeoutOccurs_ThrowsTimeoutException()
        {
            // Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SetupDelayedResponse(TimeSpan.FromSeconds(1));
            var client = new TestOAuthHttpClient(mockHandler);
            client.Timeout = TimeSpan.FromMilliseconds(100);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeoutException>(
                () => client.SendAsync(request, true, cts.Token));
            Assert.Contains("HTTP timeout", ex.Message);
        }
    }

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        public bool PreventAuthenticationCalled { get; private set; }
        private TaskCompletionSource<HttpResponseMessage> _tcs;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Check for PreventAuthentication flag set by OAuthHttpClient.PreventAuthentication()
            if (request.Options.TryGetValue(OAuthHttpMessageHandler.PreventAuthenticationOption, out bool preventAuth) && preventAuth)
            {
                PreventAuthenticationCalled = true;
            }

            if (_tcs == null)
                throw new InvalidOperationException("Handler not setup");

            try
            {
                return await _tcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        public void SetupResponse(HttpResponseMessage response)
        {
            _tcs = new TaskCompletionSource<HttpResponseMessage>();
            _tcs.SetResult(response);
        }

        public void SetupDelayedResponse(TimeSpan delay)
        {
            _tcs = new TaskCompletionSource<HttpResponseMessage>();
            _ = Task.Run(async () =>
            {
                await Task.Delay(delay).ConfigureAwait(false);
                _tcs.SetResult(new HttpResponseMessage(HttpStatusCode.OK));
            });
        }
    }

    public class TestOAuthHttpClient : OAuthHttpClient
    {
        public TestOAuthHttpClient(HttpMessageHandler handler) : base("test", "test", "https://test.com")
        {
            // Override the internal handler with our mock
            typeof(OAuthHttpClient).GetField("m_authenticator", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, handler);
        }
    }
}
