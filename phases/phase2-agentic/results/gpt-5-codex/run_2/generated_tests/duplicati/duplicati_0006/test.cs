using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Xunit;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_WhenAuthenticateIsFalse_PreventsAuthentication()
        {
            var handler = new TestOAuthHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));

            using var client = CreateClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/resource");
            using var response = await client.SendAsync(request, authenticate: false, cancellationToken: CancellationToken.None);

            Assert.True(handler.PreventAuthenticationFlag);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WhenAuthenticateIsTrue_DoesNotPreventAuthentication()
        {
            var handler = new TestOAuthHttpMessageHandler();

            using var client = CreateClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/resource");
            using var response = await client.SendAsync(request, authenticate: true, cancellationToken: CancellationToken.None);

            Assert.False(handler.PreventAuthenticationFlag);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendAsync_WhenUnderlyingSendIsCanceled_ThrowsTimeoutException()
        {
            var handler = new TestOAuthHttpMessageHandler((_, _) => throw new OperationCanceledException());

            using var client = CreateClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/resource");

            var exception = await Assert.ThrowsAsync<TimeoutException>(() => client.SendAsync(request, authenticate: true, cancellationToken: CancellationToken.None));
            Assert.Contains("HTTP timeout", exception.Message);
        }

        [Fact]
        public async Task SendAsync_WhenCancellationRequestedDuringSend_PreservesOperationCanceledException()
        {
            var cts = new CancellationTokenSource();

            var handler = new TestOAuthHttpMessageHandler((_, token) =>
            {
                cts.Cancel();
                throw new OperationCanceledException(token);
            });

            using var client = CreateClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/resource");

            await Assert.ThrowsAsync<OperationCanceledException>(() => client.SendAsync(request, authenticate: true, cancellationToken: cts.Token));
            Assert.True(cts.IsCancellationRequested);
        }

        private static OAuthHttpClient CreateClient(TestOAuthHttpMessageHandler handler)
        {
            var ctor = typeof(OAuthHttpClient).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, binder: null, new[] { typeof(OAuthHttpMessageHandler) }, modifiers: null);
            if (ctor == null)
                throw new InvalidOperationException("Could not find OAuthHttpClient constructor that accepts OAuthHttpMessageHandler.");

            return (OAuthHttpClient)ctor.Invoke(new object[] { handler });
        }

        private sealed class TestOAuthHttpMessageHandler : OAuthHttpMessageHandler
        {
            private static readonly HttpRequestOptionsKey<bool> PreventAuthenticationKey = GetPreventAuthenticationKey();

            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

            public bool PreventAuthenticationFlag { get; private set; }

            public TestOAuthHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync = null)
                : base("authid", "protocol", "https://oauth.example.com")
            {
                _sendAsync = sendAsync ?? ((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                PreventAuthenticationFlag = request.Options.TryGetValue(PreventAuthenticationKey, out var prevent) && prevent;
                return _sendAsync(request, cancellationToken);
            }

            private static HttpRequestOptionsKey<bool> GetPreventAuthenticationKey()
            {
                var field = typeof(OAuthHttpMessageHandler).GetField("PreventAuthenticationOption", BindingFlags.NonPublic | BindingFlags.Static);
                if (field == null)
                    throw new InvalidOperationException("Could not access PreventAuthenticationOption field.");

                return (HttpRequestOptionsKey<bool>)field.GetValue(null);
            }
        }
    }
}
