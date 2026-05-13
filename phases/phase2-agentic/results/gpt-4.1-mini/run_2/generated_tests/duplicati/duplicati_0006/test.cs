using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Duplicati.Library;
using Moq;
using Xunit;

namespace Duplicati.Tests.Library.Backend.OAuthHelper
{
    public class OAuthHttpClientTests
    {
        [Fact]
        public async Task SendAsync_WithAuthenticateFalse_CallsPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var client = new OAuthHttpClient(handlerMock.Object);

            var calledPreventAuthentication = false;
            handlerMock.Setup(h => h.PreventAuthentication(It.IsAny<HttpRequestMessage>()))
                .Callback<HttpRequestMessage>(req => calledPreventAuthentication = true)
                .Returns<HttpRequestMessage>(req => req);

            // Setup SendAsync to return a successful response
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var sendAsyncCalled = false;
            // We need to override SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)
            // but it's protected in HttpClient, so we use a derived class to mock it.
            var clientMock = new OAuthHttpClientMock(handlerMock.Object);
            clientMock.SetSendAsyncFunc(async (req, option, token) =>
            {
                sendAsyncCalled = true;
                return await Task.FromResult(response);
            });

            // Act
            var result = await clientMock.SendAsync(request, false, CancellationToken.None);

            // Assert
            Assert.True(calledPreventAuthentication);
            Assert.True(sendAsyncCalled);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task SendAsync_WithAuthenticateTrue_DoesNotCallPreventAuthentication()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var clientMock = new OAuthHttpClientMock(handlerMock.Object);

            var calledPreventAuthentication = false;
            handlerMock.Setup(h => h.PreventAuthentication(It.IsAny<HttpRequestMessage>()))
                .Callback(() => calledPreventAuthentication = true)
                .Returns<HttpRequestMessage>(req => req);

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var sendAsyncCalled = false;
            clientMock.SetSendAsyncFunc(async (req, option, token) =>
            {
                sendAsyncCalled = true;
                return await Task.FromResult(response);
            });

            // Act
            var result = await clientMock.SendAsync(request, true, CancellationToken.None);

            // Assert
            Assert.False(calledPreventAuthentication);
            Assert.True(sendAsyncCalled);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task SendAsync_ThrowsTimeoutException_WhenOperationCanceledExceptionAndNotCancelledByToken()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var clientMock = new OAuthHttpClientMock(handlerMock.Object);

            clientMock.SetSendAsyncFunc((req, option, token) =>
            {
                throw new OperationCanceledException();
            });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => clientMock.SendAsync(request, true, CancellationToken.None));
            Assert.Contains("HTTP timeout", ex.Message);
        }

        [Fact]
        public async Task SendAsync_ThrowsOperationCanceledException_WhenOperationCanceledExceptionAndCancelledByToken()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");
            var handlerMock = new Mock<OAuthHttpMessageHandler>("authid", "protocolKey", "oauthurl") { CallBase = true };
            var clientMock = new OAuthHttpClientMock(handlerMock.Object);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            clientMock.SetSendAsyncFunc((req, option, token) =>
            {
                throw new OperationCanceledException(null, token);
            });

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => clientMock.SendAsync(request, true, cts.Token));
        }

        // Helper derived class to mock protected SendAsync method
        private class OAuthHttpClientMock : OAuthHttpClient
        {
            private Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>>? _sendAsyncFunc;

            public OAuthHttpClientMock(OAuthHttpMessageHandler handler) : base(handler)
            {
            }

            public void SetSendAsyncFunc(Func<HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>> func)
            {
                _sendAsyncFunc = func;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
            {
                if (_sendAsyncFunc != null)
                {
                    return _sendAsyncFunc(request, completionOption, cancellationToken);
                }
                return base.SendAsync(request, completionOption, cancellationToken);
            }
        }
    }
}
